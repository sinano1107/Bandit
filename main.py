from fastapi import FastAPI
import numpy as np

app = FastAPI()


@app.get("/")
def read_root():
    return {"Hello": "World"}


@app.get("/vertices")
def read_vertices():
    return np.random.randint(3, 10 + 1)