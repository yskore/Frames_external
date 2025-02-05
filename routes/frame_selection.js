const express = require('express');
const router = express.Router();
const mongoose = require('mongoose');
const Frame= require('../models/frames');

const mongoUrl= 'mongodb+srv://frames_editor:korecool@frames1.zwwpxow.mongodb.net/frames_storage?retryWrites=true&w=majority&appName=Frames1';
const dbName= 'frames_storage';

mongoose.connect('mongodb+srv://frames_editor:korecool@frames1.zwwpxow.mongodb.net/frames_storage?retryWrites=true&w=majority&appName=Frames1', { useNewUrlParser: true, useUnifiedTopology: true });  
  const db = mongoose.connection;
  db.on('error', console.error.bind(console, 'connection error:'));
  db.once('open', () => {
    console.log('Connected to MongoDB..Pulling Frames');
  });

  router.get('/frames', async (req, res) => {
    try {
      const frames = await Frame.find();
      res.json(frames);
    } catch (e) {
      res.status(500).send('Error fetching frames');
    }
  });

  module.exports = router;