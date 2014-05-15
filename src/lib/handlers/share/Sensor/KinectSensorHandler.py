#!/usr/bin/env python
"""
=====================================
kinectSensorHandler.py - Kinect Sensor Handler
=====================================

Depending on which region this acts in, triggers signal
"""

import threading, subprocess, os, time, socket
import numpy, math
import sys
import socket
import struct

import lib.handlers.handlerTemplates as handlerTemplates

class KinectSensorHandler(handlerTemplates.SensorHandler):
    def __init__(self, executor, shared_data):
        """
        Start up sensor handler subwindow and create a new thread to listen to it.
        """

        # Since we don't want to have to poll the subwindow for each request,
        # we need a data structure to cache sensor states:
        self.sensorValue = {'XR':[], 'XL':[], 'YL':[], 'YR':[]}
        self.num_of_data_points = 5 # the number of data we keep for filtering
        if executor is not None:
            self.proj = executor.proj
        self.sensorListenInitialized = False
        self._running = True
        self.sensorListenThread = None

    def _stop(self):

        print >>sys.__stderr__, "(SENS) Terminating dummysensor GUI listen thread..."
        self._running = False
        self.sensorListenThread.join()


    def whichQuadrant(self, number, hand, initial=False):
        """
        Return a boolean value corresponding to the state of the sensor with name ``Quadrant_number``
        If such a sensor does not exist, returns ``None``

        number (int): Quadrant number
        hand (string): which hand we want to detect `l` or `r`
        """

        if initial:
            # start communication
            # Create new thread to communicate with subwindow
            
            #print self
            if self.sensorListenThread is None:
                print "(SENS) Starting Kinect listen thread..."
                self.sensorListenThread = threading.Thread(target = self._sensorListen)
                self.sensorListenThread.daemon = True
                self.sensorListenThread.start()
            
            return False
            

        else:
            # convert hand name
            if hand.lower() in ['r','right']:
                hand = 'R'
            elif hand.lower() in ['l','left']:
                hand = 'L'

            x_coord_name = 'X'+hand
            y_coord_name = 'Y'+hand
            
                
            if ((self._getCoordValue(x_coord_name) > 0) and (self._getCoordValue(y_coord_name) > 0)) and (number == 1):
               
                return True
            
            elif ((self._getCoordValue(x_coord_name) < 0) and (self._getCoordValue(y_coord_name) > 0)) and (number == 2):
                
                return True               
            
            elif ((self._getCoordValue(x_coord_name) < 0) and (self._getCoordValue(y_coord_name) < 0)) and (number == 3):
                
                return True
            
            elif ((self._getCoordValue(x_coord_name) > 0) and (self._getCoordValue(y_coord_name) < 0)) and (number == 4):
                
                return True
            else:
                return False
                
    def _getCoordValue(self, coord_name):
        """
        return the coord value of the given coord_name by applying filtering on the data
        find the median of the list of data stored
        """
        
        data = self.sensorValue[coord_name]
        if len(data) == 0: return 0
        return sorted(data)[len(data)/2]
        
            
    def _sensorListen(self):
        """
        Processes messages from the sensor handler subwindow, and updates our cache appropriately
        """
        MCAST_GRP = '239.0.0.222'
        MCAST_PORT = 2222

        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, socket.IPPROTO_UDP)
        sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

        sock.settimeout(5)
        
        try:
            sock.bind(('', MCAST_PORT))
            mreq = struct.pack("4sl", socket.inet_aton(MCAST_GRP), socket.INADDR_ANY)

            sock.setsockopt(socket.IPPROTO_IP, socket.IP_ADD_MEMBERSHIP, mreq)
        except:
            print "ERROR: Cannot bind to port.  Try killing all Python processes and trying again."
            return

        while self._running:
            # Wait for and receive a message from the subwindow
            try:
                receivedMsg = sock.recv(1024)
            except socket.timeout:
                continue

            foundMessage = False
            for ind,val in enumerate(receivedMsg):
                if val == '@' and receivedMsg[ind-1] == '#':
                    foundMessage = True
                    break  
            if not foundMessage:
                continue

            
            for i, coord_name in enumerate(['XR','XL','YR','YL']):
                ind_start = ind + 1 + 4 * i
                ind_end = ind + 5 + 4 * i
                num_byte = receivedMsg[ind_start:ind_end]
                # get the coord value of coord_name
                coord_val = struct.unpack("!i",num_byte)[0]
                # store number of data given by `self.num_of_data_point`
                if len(self.sensorValue[coord_name]) == self.num_of_data_points:
                    self.sensorValue[coord_name].pop(0)
                self.sensorValue[coord_name].append(coord_val)

            
if __name__ == "__main__":
    h = KinectSensorHandler(None,{})
    h.whichQuadrant(1,'l',True)
    
    while True:
        time.sleep(1)
        print 'L'
        print '1', h.whichQuadrant(1,'l')
        print '2', h.whichQuadrant(2,'l')
        print '3', h.whichQuadrant(3,'l')
        print '4', h.whichQuadrant(4,'l')
        print
        print 'R'
        print '1', h.whichQuadrant(1,'r')
        print '2', h.whichQuadrant(2,'r')
        print '3', h.whichQuadrant(3,'r')
        print '4', h.whichQuadrant(4,'r')
        print 
        time.sleep(1)
    
