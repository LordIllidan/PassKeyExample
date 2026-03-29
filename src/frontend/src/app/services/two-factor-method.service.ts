import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export enum TwoFactorMethodType {
  TOTP = 1,
  U2F = 2,
  SMS = 3,
  Email = 4,
  Push = 5
}

@Injectable({
  providedIn: 'root'
})
export class TwoFactorMethodService {
  private apiUrl = '/api/v1/auth/2fa/methods';

  constructor(private http: HttpClient) {}

  generateCode(userId: string, methodType: TwoFactorMethodType): Observable<{ code: string; methodType: string; expiresIn: number; message: string }> {
    return this.http.post<{ code: string; methodType: string; expiresIn: number; message: string }>(
      `${this.apiUrl}/generate`,
      { userId, methodType }
    );
  }

  verifyCode(userId: string, methodType: TwoFactorMethodType, code: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(
      `${this.apiUrl}/verify`,
      { userId, methodType, code }
    );
  }

  getUserMethods(userId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/user/${userId}`);
  }

  setupMethod(userId: string, methodType: TwoFactorMethodType, configuration?: Record<string, string>): Observable<any> {
    return this.http.post<any>(
      `${this.apiUrl}/setup`,
      { userId, methodType, configuration }
    );
  }

  setPrimaryMethod(methodId: string, userId: string): Observable<{ success: boolean }> {
    return this.http.post<{ success: boolean }>(
      `${this.apiUrl}/${methodId}/primary`,
      { userId }
    );
  }

  disableMethod(methodId: string, userId: string): Observable<{ success: boolean }> {
    return this.http.post<{ success: boolean }>(
      `${this.apiUrl}/${methodId}/disable`,
      { userId }
    );
  }
}
