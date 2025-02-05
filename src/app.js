const mongoose = require('mongoose');
const express = require('express');
const { generateAccessToken, verifyAccessToken, authenticate } = require('../tokenUtils');


const connectToDb = async () => {
    try {
      await mongoose.connect('mongodb+srv://frames_editor:korecool@frames1.zwwpxow.mongodb.net/frames_storage?retryWrites=true&w=majority&appName=Frames1');
      console.log('Successfully connected to MongoDB');
    } catch (err) {
      console.log('Failed to connect to MongoDB:', err);
    }
  };
  
  connectToDb();

const app = express();
app.use(express.json());
require('../models/user_basic'); // Import the User model
require('../models/user_profile'); // Import the user_profile model
require('../models/pieces'); // Import the Piece model
require('../models/frames'); // Import the Frame model
require('../models/followers'); // Import the followers model
require('../models/following'); // Import the following model
require('../models/anchors'); // Import the anchors model


const userRoutes = require('../routes/userroutes');
const setProfileRoutes = require('../routes/set_userprofile'); // Import the set_profile routes
const setFrameRoutes = require('../routes/new_frame'); // Import the set_frame routes
const setPieceRoutes = require('../routes/new_piece'); // Import the set_piece routes
const frameSelectionRouter = require('../routes/frame_selection'); // Import the frame_selection routes
const profilePageRouter = require('../routes/profile_page'); // Import the profile_page routes
const PieceupdateRouter = require('../routes/piece_info_updates'); // Import the update_piece routes
const AnchorRouter = require('../routes/new_anchor'); // Import the anchor routes
const FetchAnchorRouter = require('../routes/fetchanchors'); // Import the fetch_anchors routes
const liveStatusRouter = require('../routes/toggle_live_status'); // Import the toggle_live_status routes
const deletePieceRouter = require('../routes/delete_piece'); // Import the delete_piece routes





app.get('/', (req, res) => {
    res.send('Hello from App Engine!');
});

// Use the userRoutes for handling user-related routes
app.use(userRoutes);
app.use(setProfileRoutes); // Use the setProfileRoutes for handling set_profile routes
app.use(setFrameRoutes); // Use the setFrameRoutes for handling set_frame routes
app.use(setPieceRoutes); // Use the setPieceRoutes for handling set_piece routes
app.use(frameSelectionRouter); // Use the frameSelectionRouter for handling frame_selection routes
app.use(profilePageRouter); // Use the profilePageRouter for handling profile_page routes
app.use(PieceupdateRouter); // Use the PieceupdateRouter for handling update_piece routes
app.use(AnchorRouter); // Use the AnchorRouter for handling anchor routes
app.use(FetchAnchorRouter); // Use the FetchAnchorRouter for handling fetch_anchor routes
app.use(liveStatusRouter); // Use the liveStatusRouter for handling toggle_live_status routes
app.use(deletePieceRouter); // Use the deletePieceRouter for handling delete_piece routes
const PORT = process.env.PORT || 8080;
app.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
});