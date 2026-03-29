import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WebAuthnService } from '../../services/webauthn.service';

@Component({
  selector: 'app-passkey-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="passkey-register">
      <h2>Register Passkey</h2>
      <div class="form-group">
        <label for="userId">User ID:</label>
        <input 
          type="text" 
          id="userId" 
          [(ngModel)]="userId" 
          placeholder="Enter user ID (UUID)"
          class="form-control"
        />
      </div>
      <div class="form-group">
        <label for="passkeyName">Device Name (optional):</label>
        <input 
          type="text" 
          id="passkeyName" 
          [(ngModel)]="passkeyName" 
          placeholder="My Device"
          class="form-control"
        />
      </div>
      <button 
        (click)="register()" 
        [disabled]="loading || !userId"
        class="btn btn-primary"
      >
        {{ loading ? 'Registering...' : 'Register Passkey' }}
      </button>
      <div *ngIf="error" class="error">{{ error }}</div>
      <div *ngIf="success" class="success">{{ success }}</div>
    </div>
  `,
  styles: [`
    .passkey-register {
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
  `]
})
export class PasskeyRegisterComponent {
  userId = '';
  passkeyName = '';
  loading = false;
  error?: string;
  success?: string;

  constructor(private webauthn: WebAuthnService) {}

  async register() {
    if (!this.webauthn.isSupported()) {
      this.error = 'WebAuthn is not supported in this browser';
      return;
    }

    if (!this.userId) {
      this.error = 'User ID is required';
      return;
    }

    this.loading = true;
    this.error = undefined;
    this.success = undefined;

    try {
      const result = await this.webauthn.registerPasskey(this.userId, this.passkeyName);
      this.success = `Passkey registered successfully! ID: ${result.id}`;
      this.userId = '';
      this.passkeyName = '';
    } catch (error: any) {
      this.error = error.error?.error || error.message || 'Registration failed';
    } finally {
      this.loading = false;
    }
  }
}


