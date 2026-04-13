import './App.css'
import Button from "../shared/components/Button"

function App() {

  return (
    <>
    <div>
        <h1>Portfolio Management</h1>
        <Button variant="primary" size="md">Primary Button</Button>
        <Button variant="secondary" size="md">Secondary Button</Button>
        <Button variant="danger" size="md">Danger Button</Button>
        <Button variant="ghost" size="md">Ghost Button</Button>
    </div>
    </>
  )
}

export default App
