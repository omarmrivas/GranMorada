﻿import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BCM)

GPIO.setup(18, GPIO.OUT)

GPIO.output(18, GPIO.HIGH)

GPIO.output(18, GPIO.LOW)

time.sleep(0.5)

GPIO.output(18, GPIO.HIGH)

GPIO.cleanup()