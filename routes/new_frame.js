const express = require('express');
const router = express.Router();
const Frame = require('../models/frames');

router.post('/new_frame', async (req, res) => {
  const { Frame_object, Frame_owner, Frame_display, Frame_collaborators, Frame_title, Frame_description, Frame_creation_date, Frame_for_sale, Frame_price } = req.body;

  try {
    const newFrame = new Frame({
      Frame_object,
      Frame_owner,
      Frame_display,
      Frame_collaborators,
      Frame_title,
      Frame_description,
      Frame_creation_date,
      Frame_for_sale,
      Frame_price,
      Face_name
    });

    await newFrame.save();

    res.status(201).json({ success: true, message: 'New frame created successfully.', frame: newFrame });
  } catch (err) {
    console.error('Error creating new frame:', err);
    res.status(500).json({ success: false, message: 'Failed to create new frame.' });
  }
});

module.exports = router;