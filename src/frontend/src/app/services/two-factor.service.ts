import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TwoFactorService {
  private apiUrl = '/api/v1/auth/2fa';

  constructor(private http: HttpClient) {}

  startSetup(userId: string): Observable<{ secret: string; qrCodeUri: string }> {
    return this.http.post<{ secret: string; qrCodeUri: string }>(
      `${this.apiUrl}/setup/start`,
      { userId }
    );
  }

  verifySetup(userId: string, code: string): Observable<{ success: boolean; backupCodes: string[]; message: string }> {
    return this.http.post<{ success: boolean; backupCodes: string[]; message: string }>(
      `${this.apiUrl}/setup/verify`,
      { userId, code }
    );
  }

  verifyCode(userId: string, code: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(
      `${this.apiUrl}/verify`,
      { userId, code }
    );
  }

  disable(userId: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(
      `${this.apiUrl}/disable`,
      { userId }
    );
  }

  getStatus(userId: string): Observable<{ isEnabled: boolean }> {
    return this.http.get<{ isEnabled: boolean }>(
      `${this.apiUrl}/status?userId=${userId}`
    );
  }

  regenerateBackupCodes(userId: string): Observable<{ backupCodes: string[] }> {
    return this.http.post<{ backupCodes: string[] }>(
      `${this.apiUrl}/backup-codes/regenerate`,
      { userId }
    );
  }
}
