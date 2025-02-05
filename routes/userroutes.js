const express = require('express');
const router = express.Router();
const bodyParser = require('body-parser');
const user_basic = require('../models/user_basic');
const Followers = require('../models/followers');
const Following = require('../models/following');
const jwt = require('jsonwebtoken');
const { generateAccessToken, verifyAccessToken, authenticate } = require('../tokenUtils');

// Parse JSON bodies
router.use(bodyParser.json());



// Route to handle POST requests to add new users
router.post('/user_basic', async (req, res) => {
    try {
        // Extract user details from req.body
        const { username, password, firstName, lastName, dateOfBirth, country, email, phoneNumber,  userType } = req.body;

        const existingUser = await user_basic.findOne({ username });
        if (existingUser) {
            return res.status(400).json({ success: false, message: 'Username already exists' });
        }

        // Create a new user object using the extracted user details
        const newUser = new user_basic({
            username,
            password,
            firstName,
            lastName,
            dateOfBirth,
            country,
            email,
            phoneNumber,
            userType
        });

        // Save the new user to the database
        const savedUser = await newUser.save();

        // Create followers document for the new user
        const newFollowers = new Followers({
            username: savedUser.username,
            chunkIndex: 0,
            followers: []
        });
        await newFollowers.save();

        // Create following document for the new user
        const newFollowing = new Following({
            username: savedUser.username,
            chunkIndex: 0,
            following: []
        });
        await newFollowing.save();


        // Send a success response with the saved user object
        res.status(201).json({ success: true, user: savedUser });
    } catch (err) {
        console.error('Error adding new user:', err);
        // Send an error response with an appropriate error message
        if (process.env.NODE_ENV === 'development') {
            res.status(500).json({ success: false, message: err.message });
        } else {
            res.status(500).json({ success: false, message: 'Internal server error' });
        }
    }
    }
);

router.post('/user_exists', async (req, res) => {
    try {
       // Extract username and email from req.body 
       const { username, email } = req.body;

    // Check if a user with the same username already exists
    const existingUserByUsername = await user_basic.findOne({ username });
    if (existingUserByUsername) {
        console.log(`Found user with username: ${username}`); // Log the username
        return res.status(400).json({ success: false, message: 'Username already exists', exists: true });
    }

    // Check if a user with the same email already exists
    const existingUserByEmail = await user_basic.findOne({ email });
    if (existingUserByEmail) {
        console.log(`Found user with email: ${email}`); // Log the email
        return res.status(400).json({ success: false, message: 'Email already exists', exists: true });
    }
    // If no user exists with the given username or email, send a success response
    res.status(200).json({ success: true, message: 'Username and email are available', exists: false });
} catch (err) {
    console.error('Error checking username and email:', err);
    // Send an error response with an appropriate error message
    if (process.env.NODE_ENV === 'development') {
        res.status(500).json({ success: false, message: err.message, exists: false });
    } else {
        res.status(500).json({ success: false, message: 'Internal server error', exists: false });
    }
}
});



router.post('/login', async (req, res) => {
    // Get the user from the database
    
    const user = await user_basic.findOne({ "username": req.body.username });
  
    // If the user doesn't exist, send an error message
    if (!user) return res.status(400).send('Invalid username.');

    
  
    // Check if the provided password matches the stored password
    if (req.body.password !== user.password) {
        return res.status(400).send('Invalid password.');
    }
    // If the username and password are correct, send a success message
    const accessToken = generateAccessToken(user);
    res.status(200).json({message: 'Logged in successfully', accessToken: accessToken });
  });

  router.get('/user_info', async (req, res) => {
    try {
        // Extract username from req.query
        const { username } = req.query;

        // Find the user with the given username
        const user = await user_basic.findOne({ username });

        // If no such user exists, send an error response
        if (!user) {
            return res.status(404).json({ success: false, message: 'User not found' });
        }

        // Send a success response with the user's information
        res.status(200).json({ success: true, user });
    } catch (err) {
        console.error('Error getting user information:', err);
        // Send an error response with an appropriate error message
        if (process.env.NODE_ENV === 'development') {
            res.status(500).json({ success: false, message: err.message });
        } else {
            res.status(500).json({ success: false, message: 'Internal server error' });
        }
    }
});

router.get('/protected', authenticate, (req, res) => {
    res.json({ message: 'Welcome to the protected route!', user: req.user });
});

module.exports = router;
