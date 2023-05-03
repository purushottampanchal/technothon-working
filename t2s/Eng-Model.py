# load packages
import os
from flask import Flask
import pandas as pd
import numpy as np
import seaborn as sns
# load data visualization packages
import matplotlib.pyplot as plt
import seaborn as sns
from collections import Counter
#text cleaning
import neattext.functions as nfx
# machine learning model 
from sklearn.linear_model import LogisticRegression
from sklearn.naive_bayes import MultinomialNB

from sklearn.feature_extraction.text import CountVectorizer, TfidfVectorizer

from sklearn.metrics import accuracy_score, confusion_matrix, classification_report, plot_confusion_matrix
from sklearn.model_selection import train_test_split

df = pd.read_csv("emotion_data.csv")
#keyword extraction

def extract_keywords(text,num=50):
    tokens = [ token for token in text.split()]
    most_common_tokens = Counter(tokens).most_common(num)
    return dict(most_common_tokens)
emotion_list = df['Emotion'].unique().tolist()
emotion_list
df['Text'] = df['Text'].apply(nfx.remove_punctuations)
happy_list = df[df['Emotion'] == 'happy']['Text'].tolist()
# happy document
happy_docx = ' '.join(happy_list)
# extract keywords
keyword_happy = extract_keywords(happy_docx)
Xfeatures = df['Text']
ylabels = df['Emotion']
cv = CountVectorizer()
X = cv.fit_transform(Xfeatures)
X_train,X_test,y_train,y_test = train_test_split(X,ylabels,test_size=0.3, random_state=42)
#model
nv_model = MultinomialNB()
nv_model.fit(X_train,y_train)
nv_model.score(X_test,y_test)
y_pred_for_nv = nv_model.predict(X_test)
lr_model = LogisticRegression()
lr_model.fit(X_train,y_train)

def predict_emotion(sample_text, model):
    myvect = cv.transform(sample_text).toarray()
    prediction = model.predict(myvect)
    pred_proba = model.predict_proba(myvect)
    pred_precentage_for_all = dict(zip(model.classes_,pred_proba[0]))
    print(pred_precentage_for_all)
    return prediction[0]
stop_words2=[".",","]
app = Flask(__name__)

# Example of a view function that receives an argument
@app.route('/text/<data>')
def predict(data):
    
    dict1 = {}
    t=[]
    e=[]
    a=""
    for i in data.split(" "):
        
        if i.endswith(".")or i.endswith(","):
            
            a+=i+" "
            t.append(a)
            e.append(predict_emotion([a],lr_model))
            
            a=""
        else:
            
            a+=i+" "
    if a!="":
        t.append(a)
        e.append(predict_emotion([a],lr_model))
    if t==[]:
        t.append(data)
        e.append(predict_emotion([data],lr_model))
        
    dict1["text"]=t
    dict1["emotion"]=e
    return dict1

if __name__ == '__main__':
    app.run(debug=True, port=8008)
