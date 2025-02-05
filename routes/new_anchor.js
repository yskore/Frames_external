const express = require('express');
const router = express.Router();
const Anchor = require('../models/anchors');
const Piece = require('../models/pieces');
const user_profile = require('../models/user_profile');
const mongoose = require('mongoose');

router.post('/anchor', async (req, res) => {
 let retryAttempts = 3;
 let delay = 1000; // 1 second initial delay

 while (retryAttempts > 0) {
   const session = await mongoose.startSession();
   
   try {
     // Wait before starting new transaction attempt
     await new Promise(resolve => setTimeout(resolve, delay));

     session.startTransaction({
       readConcern: { level: 'snapshot' },
       writeConcern: { w: 'majority' }
     });

     const {
      anchorId,
      pieceId,
      piece_owner,
      frameName,
      faceName,
      imageUrl,
      latitude,
      longitude,
      arPosition,
      arRotation,
      localScale,          // Add this
      heightAboveCamera    // Add this
    } = req.body;
    
    const newAnchor = new Anchor({
      anchorId,
      pieceId,
      pieceOwner: piece_owner,
      frameName,
      faceName,
      imageUrl,
      location: {
        type: 'Point',
        coordinates: [longitude, latitude]
      },
      arPosition,
      arRotation,
      localScale,          // Add this
      heightAboveCamera    // Add this
    });
     await newAnchor.save({ session });

     // Update the piece's live status
     const updatedPiece = await Piece.findOneAndUpdate(
       { Piece_id: pieceId },
       { $set: { live_status: true } },
       { session, new: true }
     );

     if (!updatedPiece) {
       throw new Error(`Piece with ID ${pieceId} not found`);
     }

     // Update user profile in one operation
     const updatedUserProfile = await user_profile.findOneAndUpdate(
       { username: piece_owner },
       { $inc: { Live_pieces: 1 } },
       { session, new: true, runValidators: true }
     );
     
     if (!updatedUserProfile) {
       throw new Error(`User profile for ${piece_owner} not found`);
     }

     await session.commitTransaction();

     return res.status(201).json({
       success: true,
       message: 'Anchor created and related documents updated successfully',
       anchorId: newAnchor.anchorId,
       pieceStatus: updatedPiece.live_status,
       userLivePieces: updatedUserProfile.Live_pieces
     });

   } catch (error) {
     if (session.inTransaction()) {
       await session.abortTransaction();
     }

     // If it's a write conflict and we have retries left
     if (error.message.includes('Write conflict') && retryAttempts > 1) {
       retryAttempts--;
       delay *= 2; // Exponential backoff
       console.log(`Write conflict occurred. Retrying in ${delay}ms... (${retryAttempts} attempts left)`);
       continue;
     }

     // If it's a different error or we're out of retries
     console.error('Transaction error:', error);
     return res.status(500).json({
       success: false,
       message: 'Failed to complete anchor operation',
       error: error.message
     });
   } finally {
     await session.endSession();
   }
 }
});

module.exports = router;