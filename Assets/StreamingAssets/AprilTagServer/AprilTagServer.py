from flask import Flask, request, jsonify
import numpy as np
import cv2
from pupil_apriltags import Detector


app = Flask(__name__)

detector = Detector(families='tag36h11')


def to_vector2(x, y):
    return {"x": round(x, 2), "y": round(y, 2)}

@app.route('/detect', methods=['POST'])
def detect_apriltag():
    if 'image' not in request.files:
        return jsonify({'error': 'No image uploaded'}), 400

    file = request.files['image']
    img_bytes = file.read()
    img_array = np.frombuffer(img_bytes, np.uint8)
    image = cv2.imdecode(img_array, cv2.IMREAD_GRAYSCALE)

    detections = detector.detect(image)

    result = []
    for det in detections:
        result.append({
            'id': det.tag_id,
            'center': to_vector2(det.center[0], det.center[1]),
            'corners': [to_vector2(x, y) for (x, y) in det.corners]
        })

    return jsonify({'detections': result})

@app.route('/ping')
def ping():
    return "pong"

if __name__ == "__main__":
    app.run(host="127.0.0.1", port=5000, debug = False)