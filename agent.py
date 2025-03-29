import numpy as np

class Agent:
    def __init__(self, epsilon=0.1, action_size=10):
        self.epsilon = epsilon
        # Qs = 対応するスロットマシン(今回の場合は頂点数)の価値の推定値
        self.Qs = np.zeros(action_size)
        # ns = それぞれを選択した回数
        self.ns = np.zeros(action_size)
    
    def update(self, action, reward):
        # 対応するnに加算
        self.ns[action] += 1
        # 対応するQを更新
        self.Qs[action] += (reward - self.Qs[action]) / self.ns[action]
    
    def get_action(self):
        # epsilonの確率で探索
        if (np.random.rand() < self.epsilon):
            return np.random.randint(0, len(self.Qs))
        # 1-epsilonの確率で活用
        return np.argmax(self.Qs)
