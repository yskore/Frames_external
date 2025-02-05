//update the piece_title (piece name in the UI) field and piece_description field in the database
//Add a delete route that deletes the piece from the database

const express = require('express');
const router = express.Router();
const Piece = require('../models/pieces');



router.put('/update_piece', async (req, res) => {
    const { piece_owner, old_piece_title, new_piece_title, updated_piece_description, piece_for_sale, piece_price } = req.body;

    try {
        // If the title is being changed, check for existing pieces with the new title
        if (old_piece_title !== new_piece_title) {
            const existingPiece = await Piece.findOne({ Piece_owner: piece_owner, Piece_title: new_piece_title });
            if (existingPiece) {
                return res.status(400).json({ success: false, message: 'Failed to update piece, a user cannot have pieces with the same name' });
            }
        }

        const piece = await Piece.findOneAndUpdate(
            { Piece_owner: piece_owner, Piece_title: old_piece_title },
            {
                Piece_title: new_piece_title,
                Piece_description: updated_piece_description,
                Piece_for_sale: piece_for_sale,
                Piece_price: piece_price
            },
            { new: true, runValidators: true }
        );

        if (!piece) {
            return res.status(404).json({ success: false, message: 'Piece not found' });
        }

        res.status(200).json({ success: true, message: 'Piece updated successfully', piece });
    } catch (err) {
        console.error('Error updating piece:', err);
        res.status(500).json({ success: false, message: 'Failed to update piece' });
    }
});

router.delete('/delete_piece', async (req, res) => {
    const { piece_owner, piece_title } = req.body;

    try {
        const result = await Pieces.findOneAndDelete({ 
            Piece_owner: piece_owner, 
            Piece_title: piece_title 
        });

        if (!result) {
            return res.status(404).json({ success: false, message: 'Piece not found' });
        }

        res.status(200).json({ success: true, message: 'Piece deleted successfully' });
    } catch (err) {
        console.error('Error deleting piece:', err);
        res.status(500).json({ success: false, message: 'Failed to delete piece' });
    }
});

module.exports = router;