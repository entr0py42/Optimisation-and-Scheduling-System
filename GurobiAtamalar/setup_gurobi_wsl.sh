#!/bin/bash

# Script to install and configure Gurobi in WSL

echo "===== Setting up Gurobi in WSL ====="

# Update package lists
echo "Updating package lists..."
sudo apt-get update

# Install required packages
echo "Installing required packages..."
sudo apt-get install -y build-essential python3-pip python3-venv python3-dev wget

# Create directory for Gurobi
echo "Creating Gurobi directory..."
mkdir -p ~/gurobi

# Download Gurobi (adjust version as needed)
echo "Downloading Gurobi..."
cd ~/gurobi
wget https://packages.gurobi.com/10.0/gurobi10.0.0_linux64.tar.gz

# Extract Gurobi
echo "Extracting Gurobi..."
tar xvfz gurobi10.0.0_linux64.tar.gz
cd gurobi1000/linux64

# Set up environment variables in .bashrc
echo "Setting up environment variables..."
echo "export GUROBI_HOME=~/gurobi/gurobi1000/linux64" >> ~/.bashrc
echo "export PATH=\$PATH:\$GUROBI_HOME/bin" >> ~/.bashrc
echo "export LD_LIBRARY_PATH=\$LD_LIBRARY_PATH:\$GUROBI_HOME/lib" >> ~/.bashrc
echo "export GRB_LICENSE_FILE=\$GUROBI_HOME/gurobi.lic" >> ~/.bashrc

# Apply environment variables to current session
export GUROBI_HOME=~/gurobi/gurobi1000/linux64
export PATH=$PATH:$GUROBI_HOME/bin
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:$GUROBI_HOME/lib
export GRB_LICENSE_FILE=$GUROBI_HOME/gurobi.lic

# Set up license (Cloud license)
echo "Setting up Gurobi license..."
echo "WLSACCESSID=daa3d856-65ae-46fc-8fe2-b161380c7f91" > ~/gurobi/gurobi1000/linux64/gurobi.lic
echo "WLSSECRET=b9f8f64f-d54d-497a-942d-d50c7a374bcb" >> ~/gurobi/gurobi1000/linux64/gurobi.lic
echo "LICENSEID=2661618" >> ~/gurobi/gurobi1000/linux64/gurobi.lic

# Create a Python virtual environment for Gurobi
echo "Creating Python virtual environment..."
mkdir -p ~/gurobi_venv
python3 -m venv ~/gurobi_venv
source ~/gurobi_venv/bin/activate

# Install required Python packages in the virtual environment
echo "Installing Python dependencies..."
pip install numpy

# Install Gurobi for Python
echo "Installing Gurobi Python package..."
cd ~/gurobi/gurobi1000/linux64
python setup.py install

# Install gurobipy via pip as a fallback
echo "Installing gurobipy via pip..."
pip install gurobipy

# Create a script to activate the environment and run our optimization
echo "Creating environment activation script..."
cat > ~/gurobi_project/run_optimizer.sh << EOL
#!/bin/bash
source ~/gurobi_venv/bin/activate
export GUROBI_HOME=~/gurobi/gurobi1000/linux64
export PATH=\$PATH:\$GUROBI_HOME/bin
export LD_LIBRARY_PATH=\$LD_LIBRARY_PATH:\$GUROBI_HOME/lib
export GRB_LICENSE_FILE=\$GUROBI_HOME/gurobi.lic
cd ~/gurobi_project
python wsl_gurobi_atamalar.py
EOL

chmod +x ~/gurobi_project/run_optimizer.sh

echo "===== Gurobi setup complete ====="
echo "To run the optimization, use: ~/gurobi_project/run_optimizer.sh" 