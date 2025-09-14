import { useState, useEffect } from 'react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Shuffle, RotateCcw, Trophy } from 'lucide-react'
import './App.css'

interface BingoCell {
  value: string
  marked: boolean
}

function App() {
  const [bingoCard, setBingoCard] = useState<BingoCell[][]>([])
  const [calledNumbers, setCalledNumbers] = useState<string[]>([])
  const [currentNumber, setCurrentNumber] = useState<string>('')
  const [gameWon, setGameWon] = useState(false)
  const [availableNumbers, setAvailableNumbers] = useState<string[]>([])

  const generateBingoNumbers = () => {
    const numbers: string[] = []
    const letters = ['B', 'I', 'N', 'G', 'O']
    
    letters.forEach((letter, letterIndex) => {
      const start = letterIndex * 15 + 1
      const end = start + 14
      for (let i = start; i <= end; i++) {
        numbers.push(`${letter}${i}`)
      }
    })
    
    return numbers
  }

  const generateBingoCard = () => {
    const card: BingoCell[][] = []
    const letters = ['B', 'I', 'N', 'G', 'O']
    
    for (let col = 0; col < 5; col++) {
      const column: BingoCell[] = []
      const start = col * 15 + 1
      const end = start + 14
      const columnNumbers = []
      
      for (let i = start; i <= end; i++) {
        columnNumbers.push(i)
      }
      
      const shuffled = columnNumbers.sort(() => Math.random() - 0.5)
      
      for (let row = 0; row < 5; row++) {
        if (col === 2 && row === 2) {
          column.push({ value: 'FREE', marked: true })
        } else {
          column.push({ 
            value: `${letters[col]}${shuffled[row]}`, 
            marked: false 
          })
        }
      }
      card.push(column)
    }
    
    return card
  }

  useEffect(() => {
    setBingoCard(generateBingoCard())
    setAvailableNumbers(generateBingoNumbers())
  }, [])

  const checkWin = (card: BingoCell[][]) => {
    for (let row = 0; row < 5; row++) {
      if (card.every(col => col[row].marked)) return true
    }
    
    for (let col = 0; col < 5; col++) {
      if (card[col].every(cell => cell.marked)) return true
    }
    
    if (card.every((col, index) => col[index].marked)) return true
    if (card.every((col, index) => col[4 - index].marked)) return true
    
    return false
  }

  const markCell = (colIndex: number, rowIndex: number) => {
    if (gameWon) return
    
    const newCard = bingoCard.map((col, cIndex) =>
      col.map((cell, rIndex) => {
        if (cIndex === colIndex && rIndex === rowIndex && !cell.marked) {
          return { ...cell, marked: true }
        }
        return cell
      })
    )
    
    setBingoCard(newCard)
    
    if (checkWin(newCard)) {
      setGameWon(true)
    }
  }

  const callNumber = () => {
    if (availableNumbers.length === 0 || gameWon) return
    
    const randomIndex = Math.floor(Math.random() * availableNumbers.length)
    const number = availableNumbers[randomIndex]
    
    setCurrentNumber(number)
    setCalledNumbers(prev => [...prev, number])
    setAvailableNumbers(prev => prev.filter((_, index) => index !== randomIndex))
  }

  const resetGame = () => {
    setBingoCard(generateBingoCard())
    setCalledNumbers([])
    setCurrentNumber('')
    setGameWon(false)
    setAvailableNumbers(generateBingoNumbers())
  }

  const newCard = () => {
    setBingoCard(generateBingoCard())
    setGameWon(false)
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50 p-4">
      <div className="max-w-6xl mx-auto">
        <div className="text-center mb-8">
          <h1 className="text-4xl font-bold text-gray-800 mb-2">Dynamic Bingo</h1>
          <p className="text-gray-600">Click the squares that match the called numbers!</p>
        </div>

        <div className="grid lg:grid-cols-2 gap-8">
          {/* Bingo Card */}
          <div className="flex flex-col items-center">
            <Card className="w-full max-w-md">
              <CardHeader className="text-center pb-4">
                <CardTitle className="text-2xl font-bold">BINGO</CardTitle>
                {gameWon && (
                  <div className="flex items-center justify-center gap-2 text-green-600">
                    <Trophy className="w-5 h-5" />
                    <span className="font-semibold">BINGO! You Won!</span>
                  </div>
                )}
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-5 gap-1 mb-4">
                  {['B', 'I', 'N', 'G', 'O'].map(letter => (
                    <div key={letter} className="text-center font-bold text-lg py-2 bg-blue-100 rounded">
                      {letter}
                    </div>
                  ))}
                </div>
                <div className="grid grid-cols-5 gap-1">
                  {bingoCard.map((column, colIndex) =>
                    column.map((cell, rowIndex) => (
                      <button
                        key={`${colIndex}-${rowIndex}`}
                        onClick={() => markCell(colIndex, rowIndex)}
                        className={`
                          aspect-square text-sm font-semibold rounded transition-all duration-200
                          ${cell.marked 
                            ? 'bg-green-500 text-white shadow-lg transform scale-95' 
                            : 'bg-white hover:bg-gray-50 border-2 border-gray-200 hover:border-blue-300'
                          }
                          ${cell.value === 'FREE' ? 'bg-yellow-200 text-yellow-800' : ''}
                        `}
                        disabled={cell.marked || gameWon}
                      >
                        {cell.value === 'FREE' ? 'FREE' : cell.value.slice(1)}
                      </button>
                    ))
                  )}
                </div>
              </CardContent>
            </Card>

            <div className="flex gap-2 mt-4">
              <Button onClick={newCard} variant="outline" className="flex items-center gap-2">
                <Shuffle className="w-4 h-4" />
                New Card
              </Button>
              <Button onClick={resetGame} variant="outline" className="flex items-center gap-2">
                <RotateCcw className="w-4 h-4" />
                Reset Game
              </Button>
            </div>
          </div>

          {/* Number Caller */}
          <div className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Number Caller</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="text-center">
                  {currentNumber ? (
                    <div className="text-6xl font-bold text-blue-600 mb-2">
                      {currentNumber}
                    </div>
                  ) : (
                    <div className="text-2xl text-gray-400 mb-2">
                      Ready to start!
                    </div>
                  )}
                  <Button 
                    onClick={callNumber} 
                    disabled={availableNumbers.length === 0 || gameWon}
                    className="w-full"
                    size="lg"
                  >
                    {availableNumbers.length === 0 ? 'No More Numbers' : 'Call Next Number'}
                  </Button>
                </div>
                
                <div className="text-center text-sm text-gray-600">
                  Numbers remaining: {availableNumbers.length}
                </div>
              </CardContent>
            </Card>

            {/* Called Numbers */}
            <Card>
              <CardHeader>
                <CardTitle>Called Numbers ({calledNumbers.length})</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="flex flex-wrap gap-2 max-h-48 overflow-y-auto">
                  {calledNumbers.map((number, index) => (
                    <Badge 
                      key={index} 
                      variant={number === currentNumber ? "default" : "secondary"}
                      className="text-sm"
                    >
                      {number}
                    </Badge>
                  ))}
                  {calledNumbers.length === 0 && (
                    <p className="text-gray-400 text-sm">No numbers called yet</p>
                  )}
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    </div>
  )
}

export default App
