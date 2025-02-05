const express = require('express');
const router = express.Router();
const Anchor = require('../models/anchors');

router.post('/fetchanchors', async (req, res) => {
  try {
    const { latitude, longitude, radius = 100 } = req.body;

    const anchors = await Anchor.find({
      location: {
        $near: {
          $geometry: {
            type: 'Point',
            coordinates: [longitude, latitude]
          },
          $maxDistance: radius
        }
      }
    });

    res.status(200).json({
      success: true,
      anchors: anchors
    });

  } catch (error) {
    console.error('Error fetching anchors:', error);
    res.status(500).json({
      success: false,
      message: 'Failed to fetch anchors',
      error: error.message
    });
  }
});




router.post('/get_anchors_by_owner', async (req, res) => {
    try {
      const { username } = req.body;
      
      if (!username) {
        return res.status(400).json({
          success: false,
          message: 'Username is required'
        });
      }
  
      // Find all anchors where piece_owner matches the username
      const anchors = await Anchor.find({ pieceOwner: username });
  
      if (!anchors || anchors.length === 0) {
        return res.status(200).json([]); // Return empty array if no anchors found
      }
  
      res.status(200).json(anchors);
    } catch (error) {
      console.error('Error fetching anchors:', error);
      res.status(500).json({
        success: false,
        message: 'Failed to fetch anchors',
        error: error.message
      });
    }
  });

  router.post('/get_anchor_by_piece_id', async (req, res) => {
    try {
      const { pieceId } = req.body;
      
      if (!pieceId) {
        return res.status(400).json({
          success: false,
          message: 'Piece ID is required'
        });
      }
  
      // Find the anchor where pieceId matches
      const anchor = await Anchor.findOne({ pieceId: pieceId });
  
      if (!anchor) {
        return res.status(404).json({
          success: false,
          message: 'No anchor found for this piece'
        });
      }
  
      res.status(200).json(anchor);
    } catch (error) {
      console.error('Error fetching anchor:', error);
      res.status(500).json({
        success: false,
        message: 'Failed to fetch anchor',
        error: error.message
      });
    }
  });

module.exports = router;