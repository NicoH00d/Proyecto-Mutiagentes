class FlashPointModel(Model):
    def __init__(self, width, height, num_agents, initial_fire_positions=None, num_pois=0):
        super().__init__()
        self.grid = MultiGrid(width, height, torus=False)
        self.schedule = RandomActivation(self)
        self.running = True
        self.width = width
        self.height = height

        self.cells = [[{
            "fuego": False, 
            "humo": False, 
            "punto_interes": None, 
            "paredes": {"N": False, "S": False, "E": False, "O": False},  
            "puertas": {"N": False, "S": False, "E": False, "O": False}  
        } for _ in range(height)] for _ in range(width)]

        for i in range(num_agents):
            agent = BomberoAgent(i, self)
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            while not self.grid.is_cell_empty((x, y)):
                x = self.random.randrange(self.grid.width)
                y = self.random.randrange(self.grid.height)

            self.grid.place_agent(agent, (x, y))
            self.schedule.add(agent)

        if initial_fire_positions:
            self.initialize_fire(initial_fire_positions)

        if num_pois > 0:
            self.randomize_pois(num_pois)

        self.datacollector = DataCollector(
            model_reporters={"Step": lambda m: m.schedule.steps},
            agent_reporters={"Position": "pos", "Rescues": "rescues"}
        )

    def step(self):
        self.schedule.step()
        self.datacollector.collect(self)
        self.add_random_fire()  # Agregar fuego aleatorio en cada paso

    def get_cell_info(self, pos):
        x, y = pos
        return self.cells[x][y]

    def set_fire(self, pos):
        x, y = pos
        self.cells[x][y]["fuego"] = True

    def extinguish_fire(self, pos):
        x, y = pos
        self.cells[x][y]["fuego"] = False

    def set_smoke(self, pos):
        x, y = pos
        self.cells[x][y]["humo"] = True

    def clear_smoke(self, pos):
        x, y = pos
        self.cells[x][y]["humo"] = False

    def set_point_of_interest(self, pos, poi):
        x, y = pos
        self.cells[x][y]["punto_interes"] = poi

    def clear_point_of_interest(self, pos):
        x, y = pos
        self.cells[x][y]["punto_interes"] = None

    def set_wall(self, pos, direction):
        x, y = pos
        if direction in self.cells[x][y]["paredes"]:
            self.cells[x][y]["paredes"][direction] = True

    def clear_wall(self, pos, direction):
        x, y = pos
        if direction in self.cells[x][y]["paredes"]:
            self.cells[x][y]["paredes"][direction] = False

    def set_door(self, pos, direction):
        x, y = pos
        if direction in self.cells[x][y]["puertas"]:
            self.cells[x][y]["puertas"][direction] = True

    def clear_door(self, pos, direction):
        x, y = pos
        if direction in self.cells[x][y]["puertas"]:
            self.cells[x][y]["puertas"][direction] = False

    def initialize_walls_and_doors(self, wall_positions, door_positions):

        param wall_positions: {(0, 0): {"N": True, "S": True, "E": False, "O": False}}

        param door_positions: {(0, 0): {"N": True, "S": False, "E": False, "O": False}}

        for pos, walls in wall_positions.items():
            x, y = pos
            self.cells[x][y]["paredes"] = walls

        for pos, doors in door_positions.items():
            x, y = pos
            self.cells[x][y]["puertas"] = doors

    def initialize_fire(self, fire_positions):
      param fire_positions: [(0, 0), (1, 1)]
        for pos in fire_positions:
            self.set_fire(pos)

    def add_random_fire(self):
        empty_cells = [(x, y) for x in range(self.width) for y in range(self.height)
                       if not self.cells[x][y]["fuego"] and not self.cells[x][y]["humo"]]

        if empty_cells:
            pos = self.random.choice(empty_cells)
            self.set_fire(pos)

    def randomize_pois(self, num_pois):
        empty_cells = [(x, y) for x in range(self.width) for y in range(self.height)
                       if self.cells[x][y]["punto_interes"] is None]

        for _ in range(num_pois):
            if not empty_cells:
                break
            pos = self.random.choice(empty_cells)
            poi = self.random.choice(['victima', 'falsa_alarma'])
            self.set_point_of_interest(pos, poi)
            empty_cells.remove(pos)
