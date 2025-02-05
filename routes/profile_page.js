const express = require('express');
const router = express.Router();
const Followers = require('../models/followers'); // Adjust the path as needed
const Following = require('../models/following'); // Adjust the path as needed
const Piece = require('../models/pieces'); // Adjust the path as needed
const UserProfile = require('../models/user_profile'); // Adjust the path as needed




router.post('/getfollowercount', async (req, res) => {
    try {
        const { username } = req.body;

        if (!username) {
            return res.status(400).json({ error: 'Username is required' });
        }

        // Find all documents for the given username
        const followerDocs = await Followers.find({ username });

        let totalFollowers = 0;

        // Count followers from all documents
        followerDocs.forEach(doc => {
            totalFollowers += doc.followers.length;
        });

        res.json({ username, followerCount: totalFollowers });
    } catch (error) {
        console.error('Error in getfollowercount:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

router.post('/getfollowingcount', async (req, res) => {
    try {
        const { username } = req.body;

        if (!username) {
            return res.status(400).json({ error: 'Username is required' });
        }

        // Find all documents for the given username
        const followingDocs = await Following.find({ username });

        let totalFollowing = 0;

        // Count following from all documents
        followingDocs.forEach(doc => {
            totalFollowing += doc.following.length;
        });

        res.json({ username, followingCount: totalFollowing });
    } catch (error) {
        console.error('Error in getfollowingcount:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

router.post('/getPieceCount', async (req, res) => {
    try {
        const { username } = req.body;

        if (!username) {
            return res.status(400).json({ error: 'Username is required' });
        }

        // Count the number of pieces where Piece_owner matches the username
        const pieceCount = await Piece.countDocuments({ Piece_owner: username });

        res.json({ username, pieceCount });
    } catch (error) {
        console.error('Error in getPieceCount:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

router.post('/getpiecesbyowner', async (req, res) => {
    try {
        const { username } = req.body;

        if (!username) {
            return res.status(400).json({ error: 'Username is required' });
        }

        // Find all pieces where Piece_owner matches the username
        const pieces = await Piece.find({ Piece_owner: username });

        // Map the pieces to include all fields
        const piecesData = pieces.map(piece => ({
            Piece_Object: piece.Piece_Object,
            Piece_id: piece.Piece_id,
            Piece_owner: piece.Piece_owner,
            Piece_title: piece.Piece_title,
            Frame_name: piece.Frame_name,
            live_status: piece.live_status,
            Piece_likes: piece.Piece_likes,
            Piece_location: piece.Piece_location,
            Piece_description: piece.Piece_description,
            Piece_creation_date: piece.Piece_creation_date,
            Piece_display: piece.Piece_display,
            Piece_for_sale: piece.Piece_for_sale,
            Piece_price: piece.Piece_price
        }));

        res.json({ username, pieces: piecesData });
    } catch (error) {
        console.error('Error in getpiecesbyowner:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

router.post('/getProfile', async (req, res) => {
    try {
        const { username } = req.body;

        if (!username) {
            return res.status(400).json({ error: 'Username is required' });
        }

        // Find the user profile document for the given username
        const userProfile = await UserProfile.findOne({ username });

        if (!userProfile) {
            return res.status(404).json({ error: 'User profile not found' });
        }

        // Return the user profile data
        res.json(userProfile);
    } catch (error) {
        console.error('Error in getProfile:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});



module.exports = router;