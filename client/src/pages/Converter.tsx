import React, { useState } from 'react'
import api from '../api/ApiClient'

export default function Converter(){
  const [amount, setAmount] = useState(1)
  const [source, setSource] = useState('USD')
  const [target, setTarget] = useState('EUR')
  const [result, setResult] = useState<number | null>(null)
  const [error, setError] = useState<string | null>(null)

  const submit = async () => {
    setError(null)
    try{
      const resp = await api.post('/currency/convert', { amount, source, target })
      setResult(resp.data.converted)
    }catch(e:any){
      setError(e?.response?.data?.error || e.message)
    }
  }

  return (
    <div style={{padding:20}}>
      <h2>Convert</h2>
      <div>
        <input type="number" value={amount} onChange={e=>setAmount(Number(e.target.value))} />
        <input value={source} onChange={e=>setSource(e.target.value)} />
        <input value={target} onChange={e=>setTarget(e.target.value)} />
        <button onClick={submit}>Convert</button>
      </div>
      {result !== null && <div>Converted: {result}</div>}
      {error && <div style={{color:'red'}}>Error: {error}</div>}
    </div>
  )
}
