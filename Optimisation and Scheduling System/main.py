from flask import Flask, jsonify, request
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

# Initialize the number value
number = 42

@app.route('/api/number', methods=['GET', 'POST'])
def handle_number():
    global number
    if request.method == 'GET':
        # Return the current value of 'number'
        return jsonify({'number': number})
    
    elif request.method == 'POST':
        # Update the 'number' value from the request data
        new_number = request.json.get('number')
        if new_number is not None:
            number = new_number
            return jsonify({'message': 'Number updated successfully', 'new_number': number})
        else:
            return jsonify({'error': 'Invalid input, "number" key required'}), 400

if __name__ == '__main__':
    app.run(port=5000)
