#!/usr/bin/env python
"""
================================================================
DiffDriveSimInit.py -- Pioneer Simulation Initialization Handler
================================================================
"""

import os, sys, time,subprocess
from numpy import *

from lib.simulator.ode.pioneer import DiffDriveSim

class initHandler:
    def __init__(self, proj, calib=False):

        if True: 
            return
        
        # Start in the center of the defined initial region
        initial_region = proj.rfiold.regions[int(proj.exp_cfg_data['InitialRegion'][0])]
        initial_region = proj.rfi.regions[proj.rfi.indexOfRegionWithName(proj.regionMapping[initial_region.name][0])]
        center = initial_region.getCenter()
    
        # Load the map calibration data and the region file data to feed to the simulator
        exp_cfg_name = proj.spec_data['SETTINGS']['currentExperimentName'][0]
        exp_cfg_data = proj.getExperimentConfig(exp_cfg_name)
        coordmap_map2lab,coordmap_lab2map = proj.getCoordMaps(exp_cfg_data)
        region_calib = list(coordmap_map2lab(array([1,1])))
        regionfile = os.path.join(proj.project_root, proj.spec_data['SETTINGS']['RegionFile'][0])

        # Convert the file path to the path to the decomposed region file instead.
        k = len(regionfile)-8
        regionfile = regionfile[0:k] + "_decomposed.regions"
        # Multiply the initial pose by the calibration constants from specEditor.
        initial_pose = [region_calib[0]*center[0], region_calib[1]*center[1]]
        # Convert this initial (x,y) pose to (x,y,z) pose in the simulator.
        # The convention is (x,y) => (x,0,-y) as the simulator's y is normal to the ground.
        initial_pose_sim = [initial_pose[0], 0, -initial_pose[1]]

        pose = str(initial_pose_sim)

        # Initiate the Python ODE simulator and start the UDP socket manager.
        regc =  str(region_calib)
        UDPServer = subprocess.Popen(["python",os.path.join(proj.ltlmop_root,"lib","simulator","ode","pioneer","UDPServer.py")], stderr=subprocess.PIPE, stdin=subprocess.PIPE)
                
        drive = subprocess.Popen(["python",os.path.join(proj.ltlmop_root,"lib", "simulator","ode","pioneer", "PioneerSim.py"), regionfile,regc,pose])
        
        
    def getSharedData(self):
        # Return a dictionary of any objects that will need to be shared with
        # other handlers
        return {}

