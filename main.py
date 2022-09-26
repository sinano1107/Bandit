from fastapi import FastAPI
from agent import Agent

app = FastAPI()
agent = Agent()
prevAction = 0


@app.get("/")
def read_root():
    return {"Hello": "World"}

@app.get("/start")
def start():
    global agent
    global prevAction
    agent = Agent()
    prevAction = int(agent.get_action())
    return prevAction + 5

@app.get("/update")
def update(reward: bool):
    global prevAction
    agent.update(prevAction, reward)
    prevAction = int(agent.get_action())
    return prevAction + 5