const express = require('express');
const router = express.Router();
const Piece = require('../models/pieces');

router.post('/new_piece', async (req, res) => {
  const { Piece_id, Piece_Object, Piece_owner, Piece_title, Frame_name, live_status, Piece_likes, Piece_location, Piece_description, Piece_creation_date, Piece_display, Piece_for_sale, Piece_price } = req.body;

  try {
    // Check if a piece with the same title and owner already exists
    const existingPiece = await Piece.findOne({ Piece_owner, Piece_title });
    if (existingPiece) {
      return res.status(400).json({ success: false, message: 'Failed to create piece, a user cannot have pieces with the same name' });
    }

    const newPiece = new Piece({
      Piece_id,
      Piece_Object,
      Piece_owner,
      Piece_title,
      Frame_name,
      live_status,
      Piece_likes,
      Piece_location,
      Piece_description,
      Piece_creation_date,
      Piece_display,
      Piece_for_sale,
      Piece_price
    });

    await newPiece.save();

    res.status(201).json({ success: true, message: 'New piece created successfully.', piece: newPiece });
  } catch (err) {
    console.error('Error creating new piece:', err);
    res.status(500).json({ success: false, message: 'Failed to create new piece.' });
  }
});

module.exports = router;