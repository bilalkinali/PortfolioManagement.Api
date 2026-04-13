import { useState } from 'react'
import './App.css'
import Button from '../shared/components/Button'
import Modal from '../shared/components/Modal'

function App() {
    const [isModalOpen, setIsModalOpen] = useState(false)

  return (
    <>
    <div>
        <h1>Portfolio Management</h1>
        <Button 
            onClick={() => setIsModalOpen(true)}
            variant="primary"
            size="md">
            Primary Button
        </Button>

        <Button variant="secondary" size="md">
            Secondary Button
        </Button>

        <Button variant="danger" size="md">
            Danger Button
        </Button>

        <Button variant="ghost" size="md">
            Ghost Button
        </Button>

        <Modal
            isOpen={isModalOpen}
            onClose={() => setIsModalOpen(false)}
            title="Test Modal">
            <p>This is a test modal.</p>

            <Button
                onClick={() => setIsModalOpen(false)}
                variant="danger">
                Close
            </Button>
        </Modal>
        
    </div>
    </>
  )
}

export default App
