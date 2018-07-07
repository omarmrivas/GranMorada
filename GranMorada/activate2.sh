import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BCM)

GPIO.setup(15, GPIO.OUT)

GPIO.output(15, GPIO.HIGH)

GPIO.output(15, GPIO.LOW)

time.sleep(0.5)

GPIO.output(15, GPIO.HIGH)

GPIO.cleanup()