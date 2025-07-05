🧿 AprilTag Detector – Unity Application
A Unity-based computer vision tool for detecting AprilTags in real-time using an external Python server. This project enables seamless integration of AprilTag-based spatial tracking into Unity applications, useful for robotics, AR/VR, and computer vision workflows.

📌 Features
🎯 Real-time AprilTag detection via camera input

🔗 Unity-Python communication over local network

📡 Tag pose estimation (position and orientation)

🧩 Easily extendable for AR markers, robotics, or SLAM applications

🧠 Lightweight Python backend using OpenCV and PyAprilTag

🛠️ Tech Stack
Component	Technology
Engine	Unity (C#)
Backend	Python (Flask, OpenCV, pyAprilTag)
Communication	TCP / HTTP via UnityWebRequest
Platform	Cross-platform (Tested on Windows)

📷 How It Works
Unity sends a frame (or camera stream data) to the Python server.

Python server processes the frame to detect AprilTags using OpenCV + pyAprilTag.

Detected tag data (ID, pose, size, etc.) is returned to Unity.

Unity visualizes the detected tags or uses the data for spatial reasoning.

🧪 Demo
https://youtu.be/BIT7I3kksVU
