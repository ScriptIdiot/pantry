class Fnv:
	def __init__(self):
		self.hva = 38395996
		self.prime = 61037065

	def calculate(self, word):
		for c in word:
			self.hva = self.hva ^ ord(c)
			self.hva = (self.hva * self.prime) % 2 ** 32
		return self.hva