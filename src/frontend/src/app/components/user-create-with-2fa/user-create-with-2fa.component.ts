import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { UserService } from '../../services/user.service';
import { TwoFactorMethodService, TwoFactorMethodType } from '../../services/two-factor-method.service';

@Component({
  selector: 'app-user-create-with-2fa',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="user-create-2fa">
      <h2>Create User with 2FA</h2>
      
      <div class="form-section">
        <h3>User Information</h3>
        <div class="form-group">
          <label for="email">Email:</label>
          <input 
            type="email" 
            id="email" 
            [(ngModel)]="email" 
            placeholder="user@example.com"
            class="form-control"
            required
          />
        </div>
        <div class="form-group">
          <label for="name">Name (optional):</label>
          <input 
            type="text" 
            id="name" 
            [(ngModel)]="name" 
            placeholder="John Doe"
            class="form-control"
          />
        </div>
      </div>

      <div class="form-section">
        <h3>2FA Methods</h3>
        <p class="info-text">Select 2FA methods to enable (all methods are mocked - codes will be displayed on screen)</p>
        
        <div class="method-options">
          <label class="method-option">
            <input 
              type="checkbox" 
              [(ngModel)]="selectedMethods.totp"
              (change)="onMethodChange()"
            />
            <span>🔐 TOTP (Authenticator App)</span>
          </label>
          
          <label class="method-option">
            <input 
              type="checkbox" 
              [(ngModel)]="selectedMethods.u2f"
              (change)="onMethodChange()"
            />
            <span>🔑 U2F/WebAuthn (YubiKey)</span>
          </label>
          
          <label class="method-option">
            <input 
              type="checkbox" 
              [(ngModel)]="selectedMethods.sms"
              (change)="onMethodChange()"
            />
            <span>📱 SMS</span>
            <input 
              *ngIf="selectedMethods.sms"
              type="tel" 
              [(ngModel)]="phoneNumber"
              placeholder="+1234567890"
              class="form-control method-input"
            />
          </label>
          
          <label class="method-option">
            <input 
              type="checkbox" 
              [(ngModel)]="selectedMethods.email"
              (change)="onMethodChange()"
            />
            <span>📧 Email</span>
          </label>
          
          <label class="method-option">
            <input 
              type="checkbox" 
              [(ngModel)]="selectedMethods.push"
              (change)="onMethodChange()"
            />
            <span>🔔 Push Notification</span>
            <input 
              *ngIf="selectedMethods.push"
              type="text" 
              [(ngModel)]="deviceName"
              placeholder="My Mobile Device"
              class="form-control method-input"
            />
          </label>
        </div>
      </div>

      <button 
        (click)="create()" 
        [disabled]="loading || !email"
        class="btn btn-primary"
      >
        {{ loading ? 'Creating...' : 'Create User with 2FA' }}
      </button>

      <div *ngIf="error" class="error">{{ error }}</div>
      
      <div *ngIf="success" class="success-section">
        <div class="success">{{ success }}</div>
        <div *ngIf="createdUserId" class="user-id">
          <strong>User ID:</strong> {{ createdUserId }}
        </div>
        
        <div *ngIf="generatedCodes.length > 0" class="codes-display">
          <h3>Generated 2FA Codes (Mock - Displayed on Screen)</h3>
          <div *ngFor="let codeInfo of generatedCodes" class="code-item">
            <div class="code-header">
              <strong>{{ codeInfo.methodType }}</strong>
              <span class="code-expires">Expires in: {{ codeInfo.expiresIn }}s</span>
            </div>
            <div class="code-value">{{ codeInfo.code }}</div>
            <div class="code-message">{{ codeInfo.message }}</div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .user-create-2fa {
      max-width: 700px;
      margin: 20px auto;
      padding: 20px;
      border: 1px solid #ddd;
      border-radius: 8px;
    }
    .form-section {
      margin-bottom: 30px;
      padding: 15px;
      background: #f9f9f9;
      border-radius: 8px;
    }
    .form-section h3 {
      margin-top: 0;
      color: #333;
    }
    .info-text {
      font-size: 0.9em;
      color: #666;
      margin-bottom: 15px;
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
    .method-options {
      display: flex;
      flex-direction: column;
      gap: 15px;
    }
    .method-option {
      display: flex;
      flex-direction: column;
      gap: 8px;
      padding: 10px;
      background: white;
      border: 1px solid #ddd;
      border-radius: 4px;
      cursor: pointer;
    }
    .method-option input[type="checkbox"] {
      width: auto;
      margin-right: 8px;
    }
    .method-option span {
      font-weight: normal;
      cursor: pointer;
    }
    .method-input {
      margin-top: 8px;
    }
    .btn {
      padding: 10px 20px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      width: 100%;
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
      padding: 10px;
      background: #ffe6e6;
      border-radius: 4px;
    }
    .success-section {
      margin-top: 20px;
    }
    .success {
      color: green;
      margin-top: 10px;
      padding: 10px;
      background: #e6ffe6;
      border-radius: 4px;
    }
    .user-id {
      margin-top: 10px;
      padding: 10px;
      background-color: #f0f0f0;
      border-radius: 4px;
      word-break: break-all;
    }
    .codes-display {
      margin-top: 20px;
      padding: 15px;
      background: #fff3cd;
      border: 2px solid #ffc107;
      border-radius: 8px;
    }
    .codes-display h3 {
      margin-top: 0;
      color: #856404;
    }
    .code-item {
      margin-bottom: 15px;
      padding: 15px;
      background: white;
      border-radius: 4px;
      border: 1px solid #ffc107;
    }
    .code-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 10px;
    }
    .code-value {
      font-size: 2em;
      font-weight: bold;
      text-align: center;
      padding: 15px;
      background: #f8f9fa;
      border-radius: 4px;
      font-family: monospace;
      letter-spacing: 0.2em;
      color: #007bff;
    }
    .code-message {
      margin-top: 10px;
      font-size: 0.9em;
      color: #666;
      font-style: italic;
    }
    .code-expires {
      font-size: 0.8em;
      color: #666;
    }
  `]
})
export class UserCreateWith2FAComponent {
  email = '';
  name = '';
  phoneNumber = '+1234567890';
  deviceName = 'My Mobile Device';
  
  selectedMethods = {
    totp: false,
    u2f: false,
    sms: false,
    email: false,
    push: false
  };
  
  loading = false;
  error?: string;
  success?: string;
  createdUserId?: string;
  generatedCodes: Array<{ methodType: string; code: string; expiresIn: number; message: string }> = [];

  constructor(
    private userService: UserService,
    private twoFactorMethodService: TwoFactorMethodService
  ) {}

  onMethodChange() {
    // Reset codes when methods change
    this.generatedCodes = [];
  }

  create() {
    if (!this.email) {
      this.error = 'Email is required';
      return;
    }

    this.loading = true;
    this.error = undefined;
    this.success = undefined;
    this.createdUserId = undefined;
    this.generatedCodes = [];

    // Build 2FA methods array
    const twoFactorMethods: Array<{ methodType: number; configuration?: Record<string, string> }> = [];
    
    if (this.selectedMethods.totp) {
      twoFactorMethods.push({ methodType: TwoFactorMethodType.TOTP });
    }
    if (this.selectedMethods.u2f) {
      twoFactorMethods.push({ methodType: TwoFactorMethodType.U2F });
    }
    if (this.selectedMethods.sms) {
      twoFactorMethods.push({ 
        methodType: TwoFactorMethodType.SMS,
        configuration: { phoneNumber: this.phoneNumber }
      });
    }
    if (this.selectedMethods.email) {
      twoFactorMethods.push({ 
        methodType: TwoFactorMethodType.Email,
        configuration: { email: this.email }
      });
    }
    if (this.selectedMethods.push) {
      twoFactorMethods.push({ 
        methodType: TwoFactorMethodType.Push,
        configuration: { deviceName: this.deviceName }
      });
    }

    this.userService.createUser(this.email, this.name || undefined, undefined, twoFactorMethods).subscribe({
      next: async (result) => {
        this.success = 'User created successfully!';
        this.createdUserId = result.id;
        
        // Generate codes for all enabled methods
        if (result.twoFactorMethods && result.twoFactorMethods.length > 0) {
          for (const method of result.twoFactorMethods) {
            try {
              // Convert methodType string to enum number
              const methodTypeNum = typeof method.methodType === 'string' 
                ? TwoFactorMethodType[method.methodType as keyof typeof TwoFactorMethodType] 
                : method.methodType;
              
              const codeResult = await firstValueFrom(
                this.twoFactorMethodService.generateCode(
                  result.id,
                  methodTypeNum
                )
              );
              
              if (codeResult) {
                this.generatedCodes.push({
                  methodType: codeResult.methodType,
                  code: codeResult.code,
                  expiresIn: codeResult.expiresIn,
                  message: codeResult.message
                });
              }
            } catch (err) {
              console.error('Error generating code for method:', method.methodType, err);
            }
          }
        }
        
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to create user';
        this.loading = false;
      }
    });
  }
}
