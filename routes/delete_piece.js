const express = require('express');
const router = express.Router();
const Piece = require('../models/pieces');
const Anchor = require('../models/anchors');
const user_profile = require('../models/user_profile');
const mongoose = require('mongoose');

router.post('/delete_piece', async (req, res) => {
  const { piece_title, piece_owner } = req.body;
  const session = await mongoose.startSession();
  session.startTransaction();

  try {
    // First, find the piece to get its ID and check if it exists
    const piece = await Piece.findOne({ 
      Piece_title: piece_title,
      Piece_owner: piece_owner 
    }).session(session);

    if (!piece) {
      throw new Error('Piece not found');
    }

    // If piece is live, we need to delete the anchor and decrement live_pieces count
    if (piece.live_status) {
      // Delete the anchor document if it exists
      const deleteResult = await Anchor.deleteOne(
        { pieceId: piece.Piece_id },
        { session }
      );

      if (deleteResult.deletedCount) {
        console.log('Associated anchor deleted for piece:', piece.Piece_id);
        
        // Decrement user's live_pieces count
        const updatedUserProfile = await user_profile.findOneAndUpdate(
          { username: piece_owner },
          { $inc: { Live_pieces: -1 } },
          { session, new: true }
        );

        if (!updatedUserProfile) {
          throw new Error(`User profile for ${piece_owner} not found`);
        }
      }
    }

    // Delete the piece
    const deletePieceResult = await Piece.deleteOne(
      { _id: piece._id },
      { session }
    );

    if (!deletePieceResult.deletedCount) {
      throw new Error('Failed to delete piece');
    }

    await session.commitTransaction();

    return res.status(200).json({
      success: true,
      message: 'Piece and associated data deleted successfully',
      pieceId: piece.Piece_id
    });

  } catch (error) {
    if (session.inTransaction()) {
      await session.abortTransaction();
    }
    console.error('Error deleting piece:', error);
    return res.status(500).json({
      success: false,
      message: 'Failed to delete piece',
      error: error.message
    });
  } finally {
    session.endSession();
  }
});

module.exports = router;