
import pandas as pd
from flask_ngrok import run_with_ngrok
from flask import request, jsonify
import random as r
from flask import Flask, send_file
from flask_restful import Resource, Api
app = Flask(__name__)
api = Api(app)
# run_with_ngrok(app) 

import whisper
model = whisper.load_model("base") 

def s2t(audioFile):
  print("source ---- "+audioFile)
  res = model.transcribe(audioFile)
  #print(res["text"])
  return res["text"]


class AudioUpload(Resource):
    def post(self):
        file = request.files['audio']
        file.save(file.filename)
        res = s2t(file.filename)
        print("result - "+res)
        return {'message': res}

class AudioDownload(Resource):
    def get(self, filename):
        return send_file(filename, mimetype='audio/wav')

api.add_resource(AudioUpload, '/upload')
api.add_resource(AudioDownload, '/download/<string:filename>')
    
app.run(host='127.0.0.1', port=3000)