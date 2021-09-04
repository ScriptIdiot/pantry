from Crypto.Cipher import AES
from Crypto.Util.Padding import pad
import hashlib

class AES256:
	def __init__(self, password):
		self.key = hashlib.sha256(password.encode()).digest()
		self.iv = self.get_iv()
		self.mode = AES.MODE_CBC
		self.bs = AES.block_size

	def get_iv(self):
		return 16 * b'\x00'

	def encrypt(self, plaintext):
		cipher = AES.new(self.key, self.mode, self.iv)
		ciphertext = cipher.encrypt(pad(plaintext, self.bs))
		return ciphertext