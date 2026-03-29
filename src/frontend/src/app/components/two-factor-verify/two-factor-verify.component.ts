import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TwoFactorService } from '../../services/two-factor.service';
import { TwoFactorMethodService } from '../../services/two-factor-method.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-two-factor-verify',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="two-factor-verify">
      <h2>Two-Factor Authentication</h2>
      
      <div *ngIf="generatedCode" class="code-display">
        <p><strong>Generated Code (Mock - Displayed on Screen):</strong></p>
        <div class="code-value">{{ generatedCode }}</div>
        <p class="code-message">{{ codeMessage }}</p>
      </div>
      
      <p>Please enter the code:</p>
      
      <div class="form-group">
        <input
          type="text"
          [(ngModel)]="code"
          placeholder="000000"
          maxlength="6"
          class="form-control code-input"
          (input)="onCodeInput($event)"
          (keyup.enter)="verify()"
          autofocus
        />
      </div>

      <button 
        (click)="verify()" 
        [disabled]="loading || code.length !== 6"
        class="btn btn-primary"
      >
        {{ loading ? 'Verifying...' : 'Verify' }}
      </button>

      <div class="backup-code-link">
        <a href="#" (click)="showBackupCode = !showBackupCode; $event.preventDefault()">
          Use backup code instead
        </a>
      </div>

      <div *ngIf="showBackupCode" class="backup-code-section">
        <p>Enter your 8-digit backup code:</p>
        <input
          type="text"
          [(ngModel)]="backupCode"
          placeholder="00000000"
          maxlength="8"
          class="form-control code-input"
          (input)="onBackupCodeInput($event)"
          (keyup.enter)="verifyBackupCode()"
        />
        <button 
          (click)="verifyBackupCode()" 
          [disabled]="loading || backupCode.length !== 8"
          class="btn btn-secondary"
        >
          {{ loading ? 'Verifying...' : 'Verify Backup Code' }}
        </button>
      </div>

      <div *ngIf="error" class="error">{{ error }}</div>
    </div>
  `,
  styles: [`
    .two-factor-verify {
      max-width: 400px;
      margin: 20px auto;
      padding: 20px;
      border: 1px solid #ddd;
      border-radius: 8px;
    }
    .form-group {
      margin-bottom: 15px;
    }
    .code-input {
      text-align: center;
      font-size: 1.5em;
      letter-spacing: 0.5em;
      font-family: monospace;
      width: 100%;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 4px;
    }
    .btn {
      padding: 10px 20px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      width: 100%;
      margin-top: 10px;
    }
    .btn-primary {
      background-color: #007bff;
      color: white;
    }
    .btn-secondary {
      background-color: #6c757d;
      color: white;
    }
    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    .backup-code-link {
      margin-top: 15px;
      text-align: center;
    }
    .backup-code-link a {
      color: #007bff;
      text-decoration: none;
    }
    .backup-code-section {
      margin-top: 20px;
      padding-top: 20px;
      border-top: 1px solid #ddd;
    }
    .error {
      color: red;
      margin-top: 10px;
      text-align: center;
    }
    .code-display {
      background: #fff3cd;
      border: 2px solid #ffc107;
      border-radius: 8px;
      padding: 15px;
      margin-bottom: 20px;
    }
    .code-value {
      font-size: 2em;
      font-weight: bold;
      text-align: center;
      padding: 15px;
      background: white;
      border-radius: 4px;
      font-family: monospace;
      letter-spacing: 0.2em;
      color: #007bff;
      margin: 10px 0;
    }
    .code-message {
      font-size: 0.9em;
      color: #856404;
      text-align: center;
      margin-top: 10px;
      font-style: italic;
    }
  `]
})
export class TwoFactorVerifyComponent {
  @Input() userId!: string;
  @Input() generatedCode?: string;
  @Input() codeMessage?: string;
  @Input() methodType?: number;
  @Output() verified = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  code = '';
  backupCode = '';
  showBackupCode = false;
  loading = false;
  error?: string;

  constructor(
    private twoFactorService: TwoFactorService,
    private twoFactorMethodService: TwoFactorMethodService,
    private http: HttpClient
  ) {}

  onCodeInput(event: any) {
    this.code = event.target.value.replace(/\D/g, '');
    this.error = undefined;
  }

  onBackupCodeInput(event: any) {
    this.backupCode = event.target.value.replace(/\D/g, '');
    this.error = undefined;
  }

  verify() {
    if (this.code.length !== 6) {
      this.error = 'Please enter a 6-digit code';
      return;
    }

    this.loading = true;
    this.error = undefined;

    // Complete login with 2FA
    this.http.post<any>('/api/v1/auth/passkey/login/complete', {
      userId: this.userId,
      twoFactorCode: this.code
    }).subscribe({
      next: () => {
        this.verified.emit();
        this.loading = false;
      },
      error: (error) => {
        this.error = error.error?.error || error.message || 'Invalid code';
        this.loading = false;
        this.code = '';
      }
    });
  }

  verifyBackupCode() {
    if (this.backupCode.length !== 8) {
      this.error = 'Please enter an 8-digit backup code';
      return;
    }

    this.loading = true;
    this.error = undefined;

    // Complete login with backup code
    this.http.post<any>('/api/v1/auth/passkey/login/complete', {
      userId: this.userId,
      twoFactorCode: this.backupCode
    }).subscribe({
      next: () => {
        this.verified.emit();
        this.loading = false;
      },
      error: (error) => {
        this.error = error.error?.error || error.message || 'Invalid backup code';
        this.loading = false;
        this.backupCode = '';
      }
    });
  }
}
