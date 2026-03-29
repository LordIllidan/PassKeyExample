import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WebAuthnService } from '../../services/webauthn.service';
import { TwoFactorVerifyComponent } from '../two-factor-verify/two-factor-verify.component';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-passkey-login',
  standalone: true,
  imports: [CommonModule, FormsModule, TwoFactorVerifyComponent],
  template: `
    <div class="passkey-login">
      <h2>Login with Passkey</h2>
      <div class="form-group">
        <label for="email">Email (optional):</label>
        <input 
          type="email" 
          id="email" 
          [(ngModel)]="email" 
          placeholder="Enter email to filter credentials"
          class="form-control"
        />
      </div>
      <button 
        (click)="login()" 
        [disabled]="loading"
        class="btn btn-primary"
      >
        {{ loading ? 'Logging in...' : 'Login with Passkey' }}
      </button>
      <div *ngIf="error" class="error">{{ error }}</div>
      <div *ngIf="success" class="success">{{ success }}</div>
      
      <div *ngIf="requiresTwoFactor && userId" class="two-factor-section">
        <app-two-factor-verify 
          [userId]="userId.toString()"
          [generatedCode]="generatedCode"
          [codeMessage]="codeMessage"
          [methodType]="methodType"
          (verified)="onTwoFactorVerified()"
        ></app-two-factor-verify>
      </div>
    </div>
  `,
  styles: [`
    .passkey-login {
      max-width: 500px;
      margin: 20px auto;
      padding: 20px;
      border: 1px solid #ddd;
      border-radius: 8px;
    }
    .form-group {
      margin-bottom: 15px;
    }
    label {
      display: block;
      margin-bottom: 5px;
      font-weight: bold;
    }
    .form-control {
      width: 100%;
      padding: 8px;
      border: 1px solid #ddd;
      border-radius: 4px;
    }
    .btn {
      padding: 10px 20px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
    }
    .btn-primary {
      background-color: #007bff;
      color: white;
    }
    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    .error {
      color: red;
      margin-top: 10px;
    }
    .success {
      color: green;
      margin-top: 10px;
    }
    .two-factor-section {
      margin-top: 20px;
      padding-top: 20px;
      border-top: 1px solid #ddd;
    }
  `]
})
export class PasskeyLoginComponent {
  email = '';
  loading = false;
  error?: string;
  success?: string;
  requiresTwoFactor = false;
  userId?: string;
  generatedCode?: string;
  codeMessage?: string;
  methodType?: number;

  constructor(
    private webauthn: WebAuthnService,
    private http: HttpClient
  ) {}

  async login() {
    if (!this.webauthn.isSupported()) {
      this.error = 'WebAuthn is not supported in this browser';
      return;
    }

    this.loading = true;
    this.error = undefined;
    this.success = undefined;

    try {
      const result = await this.webauthn.loginWithPasskey(this.email || undefined);
      
      if (result.requiresTwoFactor) {
        this.requiresTwoFactor = true;
        this.userId = result.userId;
        this.generatedCode = result.generatedCode;
        this.codeMessage = result.codeMessage;
        this.methodType = result.twoFactorMethodType;
        this.loading = false;
      } else {
        this.success = `Login successful! User: ${result.email || result.userId}`;
        this.email = '';
        this.loading = false;
      }
    } catch (error: any) {
      this.error = error.error?.error || error.message || 'Login failed';
      this.loading = false;
    }
  }

  onTwoFactorVerified() {
    this.success = 'Login completed successfully!';
    this.requiresTwoFactor = false;
    this.email = '';
  }
}

