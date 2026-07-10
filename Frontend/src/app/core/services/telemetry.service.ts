import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class TelemetryService {
  private http = inject(HttpClient);
  private telemetryUrl = '/api/telemetry';

  logInfo(message: string, context?: any): void {
    const log = { level: 'INFO', message, context, timestamp: new Date() };
    console.info(`[Telemetry INFO]: ${message}`, context || '');
    this.sendToBackend(log);
  }

  logWarn(message: string, context?: any): void {
    const log = { level: 'WARN', message, context, timestamp: new Date() };
    console.warn(`[Telemetry WARN]: ${message}`, context || '');
    this.sendToBackend(log);
  }

  logError(message: string, error?: any): void {
    const log = { 
      level: 'ERROR', 
      message, 
      error: error?.message || error || 'Unknown Exception', 
      stack: error?.stack || '',
      timestamp: new Date() 
    };
    console.error(`[Telemetry ERROR]: ${message}`, error || '');
    this.sendToBackend(log);
  }

  private sendToBackend(log: any): void {
    // Send diagnostics to system logs asynchronously
    this.http.post(this.telemetryUrl, log).subscribe({
      error: (err) => {
        // Fallback: suppress telemetry connection exceptions to avoid infinite loops
        if (log.level === 'ERROR') {
          console.debug('Failed to transmit diagnostic logs to backend.', err);
        }
      }
    });
  }
}
