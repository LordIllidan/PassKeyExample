import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = '/api/v1';

  constructor(private http: HttpClient) {}

  createUser(email: string, name?: string, userName?: string, twoFactorMethods?: Array<{ methodType: number; configuration?: Record<string, string> }>): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/users`, {
      email,
      name,
      userName,
      twoFactorMethods
    });
  }

  getUser(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/users/${id}`);
  }

  getUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/users`);
  }
}

