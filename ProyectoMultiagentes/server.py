from http.server import BaseHTTPRequestHandler, HTTPServer
import logging
import json

class Cell:
    def __init__(self, x, y, wall):
        self.pos = (x, y)
        self.up = wall[0] == '1'
        self.left = wall[1] == '1'
        self.down = wall[2] == '1'
        self.right = wall[3] == '1'
        self.poi = 0
        self.fire = 0
        self.door = []
        self.entrance = False

def process_map_file(filename):
    with open(filename, 'r') as map_file:
        lines = map_file.read().splitlines()

    # Procesar las paredes (primeras 6 líneas)
    walls = []
    for line in lines[:6]:
        wall_segments = line.split()
        walls.extend(wall_segments)

    # Procesar puntos de interés (siguientes 3 líneas)
    pois = []
    for line in lines[6:9]:
        pos_poi_x = line[0]
        pos_poi_y = line[2]
        pos_poi_state = line[4]
        pois.append((pos_poi_x, pos_poi_y, pos_poi_state))

    # Procesar marcadores de fuego (siguientes 10 líneas)
    fires = []
    for line in lines[9:19]:
        pos_fire_x = line[0]
        pos_fire_y = line[2]
        fires.append((pos_fire_x, pos_fire_y))

    # Procesar puertas (siguientes 8 líneas)
    doors = []
    for line in lines[19:27]:
        pos_doorA_x = line[0]
        pos_doorA_y = line[2]
        pos_doorB_x = line[4]
        pos_doorB_y = line[6]
        doors.append(((pos_doorA_x, pos_doorA_y), (pos_doorB_x, pos_doorB_y)))

    # Procesar puntos de entrada (últimas 4 líneas)
    entrances = []
    for line in lines[27:]:
        pos_entrance_x = line[0]
        pos_entrance_y = line[2]
        entrances.append((pos_entrance_x, pos_entrance_y))

    # Inicializar las celdas
    cells = []
    for i in range(6):
        for j in range(8):
            w = walls.pop(0)
            c = Cell(i + 1, j + 1, w)
            cells.append(c)

            if (str(i + 1), str(j + 1), 'v') in pois:
                c.poi = 2
            elif (str(i + 1), str(j + 1), 'f') in pois:
                c.poi = 1

            if (str(i + 1), str(j + 1)) in fires:
                c.fire = 2

            for d in doors:
                if (str(i + 1), str(j + 1)) == d[0]:
                    c.door = d[1]

            if (str(i + 1), str(j + 1)) in entrances:
                c.entrance = True

    map_data = {}

    for c in cells:
        cell_key = f"Cell {c.pos[0]}{c.pos[1]}"
        
        if cell_key not in map_data:
            map_data[cell_key] = {
                "posicion_x": c.pos[0],
                "posicion_y": c.pos[1],
                "muro_arriba": c.up,
                "muro_izquierda": c.left,
                "muro_abajo": c.down,
                "muro_derecha": c.right,
                "punto_interes": c.poi,
                "fuego": c.fire,
                "puerta": c.door,
                "entrada": c.entrance,
                "coordenadas_poi": [],
                "coordenadas_victimas": [],
                "coordenadas_fuego": [],
                "coordenadas_entradas": []
            }
        
        if c.poi == 2:  # Víctima
            map_data[cell_key]["coordenadas_victimas"].append(c.pos)
            map_data[cell_key]["coordenadas_poi"].append(c.pos)
        elif c.poi == 1:  # Falsa alarma
                map_data[cell_key]["coordenadas_poi"].append(c.pos)
        
        if c.fire == 2:  # Fuego
            map_data[cell_key]["coordenadas_fuego"].append(c.pos)
        
        if c.entrance:
            map_data[cell_key]["coordenadas_entradas"].append(c.pos)
    
    return map_data

class Server(BaseHTTPRequestHandler):
    
    def _set_response(self):
        self.send_response(200)
        self.send_header('Content-type', 'application/json')
        self.end_headers()
        
    def do_GET(self):
        map_data = process_map_file('tablero.txt')  # Procesa el archivo de mapa llamado 'tablero.txt'
        response = json.dumps(map_data)  # Convierte los datos a JSON
        
        self._set_response()
        self.wfile.write(response.encode('utf-8'))  # Envía el JSON como respuesta

    def do_POST(self):
        self._set_response()
        self.wfile.write(json.dumps({"message": "POST request received"}).encode('utf-8'))

def run(server_class=HTTPServer, handler_class=Server, port=8585):
    logging.basicConfig(level=logging.INFO)
    server_address = ('', port)
    httpd = server_class(server_address, handler_class)
    logging.info("Starting httpd...\n")  # HTTPD es el Demonio HTTP
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:   # CTRL+C para detener el servidor
        pass
    httpd.server_close()
    logging.info("Stopping httpd...\n")

if __name__ == '__main__':
    from sys import argv
    
    if len(argv) == 2:
        run(port=int(argv[1]))
    else:
        run()
