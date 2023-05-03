import sys
import whisper
model = whisper.load_model('medium')
# file_path = "D://Speechtotext/sample.mp3"
file_path = "C://Users/omkar_randhave/Downloads/audioshashitharoor.wav"
#file_path = sys.argv[1]
result = model.transcribe(file_path)
print(result['text'])
