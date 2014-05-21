# This is a specification definition file for the LTLMoP toolkit.
# Format details are described at the beginning of each section below.


======== SETTINGS ========

Actions: # List of action propositions and their state (enabled = 1, disabled = 0)
doleftone, 0
dolefttwo, 1
doleftthree, 1
doleftfour, 0
dorightone, 1
dorighttwo, 0
dorightthree, 0
dorightfour, 1

CompileOptions:
convexify: True
parser: structured
symbolic: False
use_region_bit_encoding: True
synthesizer: jtlv
fastslow: False
decompose: True

CurrentConfigName:
left

Customs: # List of custom propositions

RegionFile: # Relative path of region description file
kinecthandsensor.regions

Sensors: # List of sensor propositions and their state (enabled = 1, disabled = 0)
leftone, 0
lefttwo, 1
leftthree, 1
leftfour, 0
rightone, 1
righttwo, 0
rightthree, 0
rightfour, 1


======== SPECIFICATION ========

RegionMapping: # Mapping between region names and their decomposed counterparts
r1 = p2
r2 = p1
others = 

Spec: # Specification in structured English
#if you you are sensing leftone then do doleftone
if you you are sensing lefttwo then do dolefttwo
if you you are sensing leftthree then do doleftthree
#if you you are sensing leftfour then do doleftfour


if you you are sensing rightone then do dorightone
#if you you are sensing righttwo then do dorighttwo
#if you you are sensing rightthree then do dorightthree
if you you are sensing rightfour then do dorightfour

visit r1
visit r2

