import { invoke } from '@tauri-apps/api'

export type PluginRequest = {
  plugin: string
  method: string
  data?: object
}

export type RouteResponse<T> = {
  error?: string
  data?: T
}

export class TauriApi {
  public static async invokePlugin<T>(request: PluginRequest): Promise<T | null> {
    let response = (await invoke('plugin_request', { request: JSON.stringify(request) })) as string
    let jsonResponse = JSON.parse(response) as RouteResponse<T>

    if (jsonResponse.error) throw new Error(jsonResponse.error)

    return jsonResponse.data ?? (null as T | null)
  }
}
