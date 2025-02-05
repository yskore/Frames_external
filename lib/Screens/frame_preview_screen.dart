import 'dart:async';
import 'dart:convert';
import 'dart:io';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:frames_app/Screens/home_screen.dart';
import 'package:frames_app/Screens/user_profile_screen.dart';
import 'package:intl/intl.dart';
import 'package:flutter/material.dart';
import 'package:flutter_unity_widget/flutter_unity_widget.dart';
import 'package:frames_app/Functions/storage_credentials.dart';
import 'package:image_picker/image_picker.dart';
import 'package:frames_app/Functions/create_piece.dart';
import 'package:frames_app/Providers/user_profile_info.dart';

class FramePreviewScreen extends ConsumerStatefulWidget {
  final String frameName;
  final String faceName;
  final String imageUrl;

  FramePreviewScreen({
    required this.frameName,
    required this.faceName,
    required this.imageUrl,
  });

  @override
  _FramePreviewScreenState createState() => _FramePreviewScreenState();
}

class _FramePreviewScreenState extends ConsumerState<FramePreviewScreen> {
  UnityWidgetController? _unityWidgetController;
  bool _isUnityLoaded = false;
  String _errorMessage = '';
  String? _preparedJsonMessage;
  bool _isUnityInitializing = false;
  bool _dataReadyToSend = false;
   bool _isLoading = true;
  bool _isCorrectSceneLoaded = false;
  bool _isSceneReady = false;
  bool _isClosing = false;





  final _formKey = GlobalKey<FormState>();
  String _pieceName = '';
  String _description = '';
  File? _image;
  final _picker = ImagePicker();

  @override
  void initState() {
    super.initState();
    _reinitializeUnity();
    prepareDataForUnity();
    ();
    
  }
    @override
  void dispose() {
    // Dispose of the Unity widget controller
    _unityWidgetController?.dispose();
    
    // Set the controller to null after disposing
    _unityWidgetController = null;

    // Always call super.dispose() at the end
    super.dispose();
  }

  void prepareDataForUnity() {
    try {
      _preparedJsonMessage = jsonEncode({
        'frameName': widget.frameName,
        'faceName': widget.faceName,
        'imageUrl': widget.imageUrl,
      });
           setState(() {
        _dataReadyToSend = true;
      });
      print('Data prepared for Unity: $_preparedJsonMessage');
    } catch (e) {
      setErrorMessage('Error preparing data for Unity: $e');
    }


  }
    void _checkActiveScene() {
    _unityWidgetController?.postMessage(
      'SceneLoader',
      'GetActiveScene',
      ''
    );
  }

  void _reinitializeUnity() {
    print('Flutter: Reinitializing Unity...switching to frames_test scene');
    _unityWidgetController?.postMessage(
      'SceneLoader',
      'LoadSceneByName',
      'frames_test'
    );
    setState(() {
      _isUnityLoaded = false;
      _unityWidgetController = null;
    });
    print('Unity reinitialized: _isUnityLoaded = $_isUnityLoaded , _unityWidgetController = $_unityWidgetController');
  }
      void _setSceneReady() {
    setState(() {
      _isLoading = false;
      _isCorrectSceneLoaded = true;
      _isSceneReady = true;
    });
    ();
    print('_setSceneReady called ');
  }

   void _switchToFramesTestScene() {
    _unityWidgetController?.postMessage(
      'SceneLoader',
      'LoadSceneByName',
      'frames_test'
    );
    print('_switchToFramesTestScene called');
  }
  Future getImage() async {
    final pickedFile = await _picker.pickImage(source: ImageSource.gallery);
    setState(() {
      if (pickedFile != null) {
        _image = File(pickedFile.path);
      } else {
        print('No image selected.');
      }
    });
  }
    Future<void> _handleClosing(username) async {
        if (_isClosing) return;  // Prevent multiple closing attempts

        setState(() {
      _isClosing = true;
    });

      print("Handling closiing and resetting Unity scene");
      try {

      _unityWidgetController?.postMessage(
        'GameManager',
        'ResetUnityScene',
        'reset'
      );
      print('Piece preview popped and scene resetting...');

      await Future.delayed(const Duration(milliseconds: 500));
      
      if (_unityWidgetController != null) {
        _unityWidgetController?.dispose();
        _unityWidgetController = null;
      }

      if (mounted) {

      Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (context) => HomeScreen(username: username),
                      ),
                    );
      print('Piece preview popped and scene reset!');
    }

    } catch (e) {
      print('Error handling closing: $e');
      if (mounted) {
        Navigator.of(context).pop();
      }
    }
    
  }

  @override
  Widget build(BuildContext context) {
    final userProfileInfo = ref.watch(userProfileInfoProvider);
    final username = userProfileInfo?.username ?? 'No username';


     
    return PopScope(
    canPop: false, // Prevent automatic popping
      onPopInvoked: (didPop) async {
        if (didPop) return;

        await _handleClosing(username);
      },
    child:
    Scaffold(
      appBar: AppBar(
        title: Text('Frame Preview'),
        leading: IconButton(
          icon: Icon(Icons.arrow_back),
          onPressed: ()  {
            _handleClosing(username); 
            
          },
        ),
      ),
      body: Column(
        children: [
          if (_errorMessage.isNotEmpty)
            Padding(
              padding: const EdgeInsets.all(8.0),
              child: Text(
                _errorMessage,
                style: TextStyle(color: Colors.red),
              ),
            ),
              ElevatedButton(

            child: Text(_isUnityLoaded ? 'Reload Unity' : 'Load Unity'),

            onPressed: _isUnityInitializing || !_dataReadyToSend ? null : _loadUnity,

          ),
          Container(
            height: 300, // Adjust as needed
            child: _isUnityLoaded && _dataReadyToSend
                ? UnityWidget(
                    onUnityCreated: onUnityCreated,
                    onUnityMessage: _onUnityMessage,
                    onUnitySceneLoaded: onUnitySceneLoaded,
                    useAndroidViewSurface: true,
                    fullscreen: false,
                  )
                : Center(child: Text('Press "Load Unity" to start')),
          ),
          Expanded(
            child: SingleChildScrollView(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Form(
                  key: _formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      TextFormField(
                        decoration: InputDecoration(labelText: 'Piece Name'),
                        validator: (value) {
                          if (value == null || value.isEmpty) {
                            return 'Please enter a name for the piece';
                          }
                          return null;
                        },
                        onSaved: (value) {
                          _pieceName = value!;
                        },
                      ),
                      TextFormField(
                        decoration: InputDecoration(labelText: 'Description'),
                        maxLines: 3,
                        validator: (value) {
                          if (value == null || value.isEmpty) {
                            return 'Please enter a description';
                          }
                          return null;
                        },
                        onSaved: (value) {
                          _description = value!;
                        },
                      ),
                      SizedBox(height: 20),
                      Text('Piece Display Picture'),
                      SizedBox(height: 10),
                      _image == null
                          ? Text('No image selected.')
                          : Image.file(_image!),
                      ElevatedButton(
                        onPressed: getImage,
                        child: Text('Pick Image'),
                      ),
                      SizedBox(height: 20),
                      Center(
                        child: ElevatedButton(
                          onPressed: () async {
                            await _savePiece(userProfileInfo);
                            _resetUnityScene();
                            Future.delayed(Duration(milliseconds: 100), () {
                              Navigator.of(context).pushReplacement(
                                MaterialPageRoute(
                                  builder: (context) => UserProfileScreen(username: userProfileInfo!.username),
                                ),
                              );
                            });
                          },
                          child: Text('Save & Continue'),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    )
    );
  }
  void _loadUnity() {
    setState(() {
      _isUnityInitializing = true;
      _isUnityLoaded = true;
      _errorMessage = '';
    });

  }


  void onUnityCreated(UnityWidgetController controller) {
    print('Unity Widget created - controller: $controller');
    _unityWidgetController = controller;
    setState(() {
      _isUnityLoaded = true;
    });
     _checkActiveScene();
     print('Checking active scene');

    sendFrameDataToUnity();
  }

  void sendFrameDataToUnity() {
    if (_unityWidgetController == null || _preparedJsonMessage == null) {
      setErrorMessage('Unity controller is not initialized or data is not prepared');
      return;
    }

    try {
      _unityWidgetController!.postMessage(
        'GameManager',
        'ReceiveDataFromFlutter',
        _preparedJsonMessage!,
      );
      print('Data sent to Unity successfully');
    } catch (e) {
      setErrorMessage('Error sending data to Unity: $e');
    }
  }

  void _onUnityMessage(message) {
    print('Received message from Unity: $message');

    if (message.toString().startsWith('ACTIVE_SCENE:')) {
      String activeScene = message.toString().split(':')[1];
      if (activeScene == 'frames_test') {
        setState(() {
         _setSceneReady();
        });
        print('Current scene loaded: $activeScene');
      } else if (activeScene != 'frames_test') {
        _switchToFramesTestScene();
        print('Current scene loaded: $activeScene');
      }
    } else if (message == 'SCENE_SWITCHED') {
      setState(() {
        _setSceneReady();
      });
      print('SCENE_SWITCHED to frames_test');
      sendFrameDataToUnity();
    }
  }

  void onUnitySceneLoaded(SceneLoaded? scene) {
    
    if (scene != null) {
      print('Unity scene loaded: ${scene.name}');
    } else {
      setErrorMessage('Failed to load Unity scene');
    }
  }

  void setErrorMessage(String message) {
    setState(() {
      _errorMessage = message;
    });
    print('Error: $message');
  }

   void _resetUnityScene() {
  print(' (_resetUnityScene Called) Flutter: Resetting Unity scene...');
  _unityWidgetController?.postMessage(
    'GameManager',
    'ResetUnityScene',
    'reset'
  );
   
}
Future<void> _savePiece(UserProfileInfo? userProfileInfo) async {
    if (_formKey.currentState!.validate()) {
       if (userProfileInfo == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('User information not available. Please try again.')),
        );
        return;
      }
      _formKey.currentState!.save();
      
      if (_image == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Please select a display picture')),
        );
        return;
      }
      
      const bucketName = 'x-fabric-419423.appspot.com';
      const folderName = 'piece_display';
      
      try {
        final imageURL = await uploadImage(_image!, bucketName, folderName);
        
        // TODO: Save the piece data along with the imageURL
        // This is where you would save the _pieceName, _description, and imageURL
        // to your database or state management system

  final serializedObjectData = jsonEncode({
  'frameName': widget.frameName,
  'faceName': widget.faceName,
  'imageUrl': widget.imageUrl,
  'position': {'x': 0, 'y': 0, 'z': 0},
  'rotation': {'x': 0, 'y': 0, 'z': 0, 'w': 1},
  'scale': {'x': 1, 'y': 1, 'z': 1},
  'geolocation': {
    'latitude': 0.0,  // Will be updated when placed in AR
    'longitude': 0.0  // Will be updated when placed in AR
  }
});
        


        print('Piece display picture uploaded successfully. URL: $imageURL');
        createPiece(serializedObjectData, userProfileInfo.username, _pieceName , widget.frameName, "false", "0", "no location", _description, DateFormat('yyyy-MM-dd HH:mm:ss').format(DateTime.now()),imageURL, "false", "0" );
        
        // TODO: Implement the logic to save the 3D model data
        // This might involve sending a message to Unity to prepare the data
        // and then uploading it to your server or cloud storage
        
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Piece saved successfully!')),
        );
        
        // TODO: Navigate to the next screen or close this screen
        // Navigator.of(context).pop();
      } catch (e) {
        print('Error uploading piece display picture: $e');
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Failed to save piece. Please try again.')),
        );
      }
    }
  }


 
}