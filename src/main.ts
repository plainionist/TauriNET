import { TauriApi } from './TauriApi'

let greetInputEl: HTMLInputElement | null
let greetMsgEl: HTMLElement | null

async function login(user: string): Promise<string | null> {
  let userData = { user: user, pass: 'Hmm...' }

  try {
    return await TauriApi.invokePlugin<string>({ plugin: 'TestApp.PlugIn', method: 'login', data: userData })
  } catch (error) {
    return 'ERR: ' + error
  }
}

window.addEventListener('DOMContentLoaded', () => {
  greetInputEl = document.querySelector('#greet-input')
  greetMsgEl = document.querySelector('#greet-msg')
  document.querySelector('#greet-form')?.addEventListener('submit', async (e) => {
    e.preventDefault()
    if (greetMsgEl) greetMsgEl.innerHTML = (await login(greetInputEl?.value ?? '')) ?? 'null'
  })
})
