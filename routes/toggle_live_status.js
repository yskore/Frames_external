const express = require('express');
const router = express.Router();
const Piece = require('../models/pieces');
const Anchor = require('../models/anchors');
const user_profile = require('../models/user_profile');  // Changed to match model export
const mongoose = require('mongoose');

router.post('/toggle_live_status', async (req, res) => {
  const { piece_id, live_status } = req.body;
  const session = await mongoose.startSession();
  session.startTransaction();

  try {
    // First, find the piece to get the owner
    const piece = await Piece.findOne({ Piece_id: piece_id }).session(session);
    
    if (!piece) {
      throw new Error('Piece not found');
    }

    const piece_owner = piece.Piece_owner;  // Changed to match your schema

    // Handle setting live_status to false
    if (live_status === false) {
      // Delete the anchor document with matching pieceId
      const deleteResult = await Anchor.deleteOne(
        { pieceId: piece_id },
        { session }
      );
      
      if (!deleteResult.deletedCount) {
        console.log('No anchor found to delete for piece:', piece_id);
      }

      // Update piece's live_status to false
      const updatedPiece = await Piece.findOneAndUpdate(
        { Piece_id: piece_id },
        { live_status: false },
        { session, new: true }
      );

      console.log('Attempting to update Live_pieces for user:', piece_owner);
      
      // Decrement user's live_pieces count
      const updatedUserProfile = await user_profile.findOneAndUpdate(
        { username: piece_owner },
        { $inc: { Live_pieces: -1 } },  // Changed to match schema
        { session, new: true }
      );

      if (!updatedUserProfile) {
        throw new Error(`User profile for ${piece_owner} not found`);
      }

      await session.commitTransaction();

      return res.status(200).json({
        success: true,
        message: 'Piece set to inactive, anchor removed, and user profile updated',
        piece: updatedPiece,
        userLivePieces: updatedUserProfile.Live_pieces  // Changed to match schema
      });
    }

    // Handle setting live_status to true
    if (live_status === true) {
      const updatedPiece = await Piece.findOneAndUpdate(
        { Piece_id: piece_id },
        { live_status: true },
        { session, new: true }
      );

      await session.commitTransaction();

      return res.status(200).json({
        success: true,
        message: 'Piece set to active',
        piece: updatedPiece
      });
    }

    throw new Error('Invalid live_status value. Must be true or false.');

  } catch (error) {
    if (session.inTransaction()) {  // Added check before aborting
      await session.abortTransaction();
    }
    console.error('Error toggling live status:', error);
    return res.status(500).json({
      success: false,
      message: 'Failed to toggle piece live status',
      error: error.message
    });
  } finally {
    session.endSession();
  }
});

module.exports = router;