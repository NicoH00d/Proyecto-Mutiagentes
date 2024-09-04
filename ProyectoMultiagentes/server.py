from http.server import BaseHTTPRequestHandler, HTTPServer
import logging
import json

# Lee el archivo JSON generado por flashpoint.py
def read_simulation_state(filename='simulation_state.json'):
    try:
        with open(filename, 'r') as json_file:
            data = json.load(json_file)
        return data
    except FileNotFoundError:
        return {"error": "simulation_state.json not found"}

class Server(BaseHTTPRequestHandler):

    def _set_response(self):
        self.send_response(200)
        self.send_header('Content-type', 'application/json')
        self.end_headers()

    def do_GET(self):
        # Leer el estado actual de la simulación desde el archivo JSON
        simulation_state = read_simulation_state()
        
        # Enviar el contenido del JSON como respuesta
        self._set_response()
        self.wfile.write(json.dumps(simulation_state).encode('utf-8'))

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
