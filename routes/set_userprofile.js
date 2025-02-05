const express = require('express');
const router = express.Router();
const user_profile = require('../models/user_profile'); // assuming user_profile model is in the same directory

router.post('/set_profile', async (req, res) => {
    const { username } = req.body;

    try {
        let user = await user_profile.findOne({ Username: username });

        if (!user) {
            user = new user_profile({
                Username: username,
                Profile_photo: '',
                User_bio: '',
                Paypal_email: '',
                Frame_count: 0,
                Like_count: 0,
                User_followers: [],
                User_following: [],
                Live_pieces: 0,
                Is_premium: false,
            });

            await user.save();
        }

        res.status(200).json({ success: true, user });
    } catch (err) {
        console.error('Error setting user profile:', err);
        res.status(500).json({ success: false, message: 'Internal server error' });
    }
});


router.post('/update_profile', async (req, res) => {
    const { username, Bio, imageUrl } = req.body;

    try {
        const user = await user_profile.findOne({ Username: username });

        if (!user) {
            return res.status(404).json({ success: false, message: 'User not found' });
        }

        user.User_bio = Bio || user.User_bio;
        user.Profile_photo = imageUrl || user.Profile_photo;

        await user.save();

        res.status(200).json({ success: true, message: 'User Profile Updated successfully.' });
    } catch (err) {
        console.error('Error updating user profile:', err);
        res.status(500).json({ success: false, message: 'Failed to update user Profile.' });
    }
});

module.exports = router;