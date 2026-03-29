import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TwoFactorService } from '../../services/two-factor.service';

@Component({
  selector: 'app-two-factor-setup',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="two-factor-setup">
      <h2>Setup Two-Factor Authentication</h2>
      
      <div *ngIf="!secret && !qrCodeUri" class="setup-step">
        <p>Click the button below to start setting up two-factor authentication.</p>
        <button (click)="startSetup()" [disabled]="loading" class="btn btn-primary">
          {{ loading ? 'Starting...' : 'Start Setup' }}
        </button>
      </div>

      <div *ngIf="secret && qrCodeUri && !isVerified" class="setup-step">
        <h3>Step 1: Scan QR Code</h3>
        <p>Scan this QR code with your authenticator app (Google Authenticator, Microsoft Authenticator, etc.)</p>
        <div class="qr-code-container">
          <img [src]="qrCodeImageUrl" alt="QR Code" class="qr-code" />
        </div>
        <p class="secret-text">Or enter this secret manually: <code>{{ secret }}</code></p>

        <h3>Step 2: Verify Setup</h3>
        <p>Enter the 6-digit code from your authenticator app to verify:</p>
        <div class="form-group">
          <input
            type="text"
            [(ngModel)]="verificationCode"
            placeholder="000000"
            maxlength="6"
            class="form-control code-input"
            (input)="onCodeInput($event)"
          />
        </div>
        <button 
          (click)="verifySetup()" 
          [disabled]="loading || verificationCode.length !== 6"
          class="btn btn-primary"
        >
          {{ loading ? 'Verifying...' : 'Verify & Enable' }}
        </button>
      </div>

      <div *ngIf="isVerified && backupCodes.length > 0" class="setup-step">
        <h3>Setup Complete!</h3>
        <div class="backup-codes">
          <p><strong>Important:</strong> Save these backup codes in a safe place. You can use them to access your account if you lose your device.</p>
          <div class="codes-list">
            <div *ngFor="let code of backupCodes" class="code-item">{{ code }}</div>
          </div>
          <button (click)="copyBackupCodes()" class="btn btn-secondary">Copy Codes</button>
        </div>
        <button (click)="close()" class="btn btn-primary">Done</button>
      </div>

      <div *ngIf="error" class="error">{{ error }}</div>
    </div>
  `,
  styles: [`
    .two-factor-setup {
      max-width: 600px;
      margin: 20px auto;
      padding: 20px;
      border: 1px solid #ddd;
      border-radius: 8px;
    }
    .setup-step {
      margin-bottom: 30px;
    }
    .qr-code-container {
      text-align: center;
      margin: 20px 0;
      padding: 20px;
      background: #f5f5f5;
      border-radius: 8px;
    }
    .qr-code {
      max-width: 300px;
      height: auto;
    }
    .secret-text {
      margin-top: 15px;
      font-size: 0.9em;
      color: #666;
    }
    .secret-text code {
      background: #f5f5f5;
      padding: 4px 8px;
      border-radius: 4px;
      font-family: monospace;
    }
    .form-group {
      margin-bottom: 15px;
    }
    .code-input {
      text-align: center;
      font-size: 1.5em;
      letter-spacing: 0.5em;
      font-family: monospace;
      width: 200px;
      margin: 0 auto;
      display: block;
    }
    .backup-codes {
      background: #fff3cd;
      border: 1px solid #ffc107;
      border-radius: 8px;
      padding: 20px;
      margin: 20px 0;
    }
    .codes-list {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 10px;
      margin: 15px 0;
    }
    .code-item {
      background: white;
      padding: 10px;
      border-radius: 4px;
      text-align: center;
      font-family: monospace;
      font-weight: bold;
    }
    .btn {
      padding: 10px 20px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      margin: 5px;
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
    .error {
      color: red;
      margin-top: 10px;
    }
  `]
})
export class TwoFactorSetupComponent {
  @Input() userId!: string;
  
  secret = '';
  qrCodeUri = '';
  qrCodeImageUrl = '';
  verificationCode = '';
  isVerified = false;
  backupCodes: string[] = [];
  loading = false;
  error?: string;

  constructor(private twoFactorService: TwoFactorService) {}

  startSetup() {
    this.loading = true;
    this.error = undefined;

    this.twoFactorService.startSetup(this.userId).subscribe({
      next: (response) => {
        this.secret = response.secret;
        this.qrCodeUri = response.qrCodeUri;
        // Generate QR code image URL using a QR code service
        this.qrCodeImageUrl = `https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=${encodeURIComponent(response.qrCodeUri)}`;
        this.loading = false;
      },
      error: (error) => {
        this.error = error.error?.error || error.message || 'Failed to start setup';
        this.loading = false;
      }
    });
  }

  onCodeInput(event: any) {
    // Only allow digits
    this.verificationCode = event.target.value.replace(/\D/g, '');
  }

  verifySetup() {
    if (this.verificationCode.length !== 6) {
      this.error = 'Please enter a 6-digit code';
      return;
    }

    this.loading = true;
    this.error = undefined;

    this.twoFactorService.verifySetup(this.userId, this.verificationCode).subscribe({
      next: (response) => {
        this.isVerified = true;
        this.backupCodes = response.backupCodes;
        this.loading = false;
      },
      error: (error) => {
        this.error = error.error?.error || error.message || 'Invalid verification code';
        this.loading = false;
      }
    });
  }

  copyBackupCodes() {
    const codesText = this.backupCodes.join('\n');
    navigator.clipboard.writeText(codesText).then(() => {
      alert('Backup codes copied to clipboard!');
    });
  }

  close() {
    // Emit event or handle close
    window.location.reload();
  }
}
