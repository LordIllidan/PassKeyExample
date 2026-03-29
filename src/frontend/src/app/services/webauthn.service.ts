import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, from } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class WebAuthnService {
  private apiUrl = '/api/v1';

  constructor(private http: HttpClient) {}

  isSupported(): boolean {
    return typeof window.PublicKeyCredential !== 'undefined';
  }

  async startRegistration(userId: string): Promise<PublicKeyCredentialCreationOptions> {
    const response = await this.http
      .post<any>(`${this.apiUrl}/auth/passkey/register/start`, { userId })
      .toPromise();

    // Convert DTO to WebAuthn format
    return {
      challenge: this.base64ToArrayBuffer(response.challenge),
      rp: response.rp,
      user: {
        ...response.user,
        id: this.base64ToArrayBuffer(response.user.id)
      },
      pubKeyCredParams: response.pubKeyCredParams,
      authenticatorSelection: response.authenticatorSelection,
      timeout: response.timeout,
      attestation: response.attestation
    };
  }

  async finishRegistration(
    credential: PublicKeyCredential,
    userId: string,
    name?: string
  ): Promise<any> {
    const response = credential.response as AuthenticatorAttestationResponse;

    const request = {
      userId,
      response: JSON.stringify({
        id: credential.id,
        rawId: this.arrayBufferToBase64(credential.rawId),
        response: {
          clientDataJSON: this.arrayBufferToBase64(response.clientDataJSON),
          attestationObject: this.arrayBufferToBase64(response.attestationObject)
        },
        type: credential.type
      }),
      name,
      deviceType: 'platform'
    };

    return this.http
      .post<any>(`${this.apiUrl}/auth/passkey/register/finish`, request)
      .toPromise();
  }

  async registerPasskey(userId: string, name?: string): Promise<any> {
    if (!this.isSupported()) {
      throw new Error('WebAuthn is not supported in this browser');
    }

    try {
      const options = await this.startRegistration(userId);
      const credential = await navigator.credentials.create({
        publicKey: options
      }) as PublicKeyCredential;

      return await this.finishRegistration(credential, userId, name);
    } catch (error: any) {
      console.error('Passkey registration failed:', error);
      throw error;
    }
  }

  async startLogin(email?: string): Promise<{ options: PublicKeyCredentialRequestOptions; challengeBase64: string }> {
    const response = await this.http
      .post<any>(`${this.apiUrl}/auth/passkey/login/start`, email ? { email } : {})
      .toPromise();

    return {
      options: {
        challenge: this.base64ToArrayBuffer(response.challenge),
        rpId: response.rpId,
        allowCredentials: response.allowCredentials?.map((cred: any) => ({
          ...cred,
          id: this.base64ToArrayBuffer(cred.id)
        })),
        userVerification: response.userVerification,
        timeout: response.timeout
      },
      challengeBase64: response.challenge
    };
  }

  async finishLogin(credential: PublicKeyCredential, challengeBase64: string): Promise<any> {
    const response = credential.response as AuthenticatorAssertionResponse;

    const request = {
      credentialId: this.arrayBufferToBase64(credential.rawId),
      response: JSON.stringify({
        id: credential.id,
        rawId: this.arrayBufferToBase64(credential.rawId),
        response: {
          clientDataJSON: this.arrayBufferToBase64(response.clientDataJSON),
          authenticatorData: this.arrayBufferToBase64(response.authenticatorData),
          signature: this.arrayBufferToBase64(response.signature),
          userHandle: response.userHandle ? this.arrayBufferToBase64(response.userHandle) : null
        },
        type: credential.type
      }),
      challengeBase64
    };

    return this.http
      .post<any>(`${this.apiUrl}/auth/passkey/login/finish`, request)
      .toPromise();
  }

  async loginWithPasskey(email?: string): Promise<any> {
    if (!this.isSupported()) {
      throw new Error('WebAuthn is not supported in this browser');
    }

    try {
      const { options, challengeBase64 } = await this.startLogin(email);
      
      const credential = await navigator.credentials.get({
        publicKey: options
      }) as PublicKeyCredential;

      return await this.finishLogin(credential, challengeBase64);
    } catch (error: any) {
      console.error('Passkey login failed:', error);
      throw error;
    }
  }

  getPasskeys(userId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/auth/passkey?userId=${userId}`);
  }

  deletePasskey(id: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/auth/passkey/${id}?userId=${userId}`);
  }

  private arrayBufferToBase64(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.byteLength; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary);
  }

  private base64ToArrayBuffer(base64: string): ArrayBuffer {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
      bytes[i] = binary.charCodeAt(i);
    }
    return bytes.buffer;
  }
}

