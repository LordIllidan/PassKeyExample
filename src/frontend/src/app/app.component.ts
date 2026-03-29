import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PasskeyRegisterComponent } from './components/passkey-register/passkey-register.component';
import { PasskeyLoginComponent } from './components/passkey-login/passkey-login.component';
import { UserCreateComponent } from './components/user-create/user-create.component';
import { UserCreateWith2FAComponent } from './components/user-create-with-2fa/user-create-with-2fa.component';
import { TwoFactorSetupComponent } from './components/two-factor-setup/two-factor-setup.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, UserCreateComponent, UserCreateWith2FAComponent, PasskeyRegisterComponent, PasskeyLoginComponent, TwoFactorSetupComponent],
  template: `
    <div class="app-container">
      <header>
        <h1>Passkey Authentication Demo</h1>
      </header>
      <main>
        <div class="sections">
          <section>
            <app-user-create></app-user-create>
          </section>
          <section>
            <app-user-create-with-2fa></app-user-create-with-2fa>
          </section>
          <section>
            <app-passkey-register></app-passkey-register>
          </section>
          <section>
            <app-passkey-login></app-passkey-login>
          </section>
          <section>
            <h3>Setup 2FA for Existing User</h3>
            <p>Enter User ID to setup 2FA:</p>
            <input 
              type="text" 
              [(ngModel)]="userIdFor2FA" 
              placeholder="User ID (UUID)"
              class="form-control"
              style="margin-bottom: 10px;"
            />
            <app-two-factor-setup *ngIf="userIdFor2FA" [userId]="userIdFor2FA"></app-two-factor-setup>
          </section>
        </div>
      </main>
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      background-color: #f5f5f5;
    }
    header {
      background-color: #007bff;
      color: white;
      padding: 20px;
      text-align: center;
    }
    main {
      padding: 20px;
    }
    .sections {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
      gap: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }
    section {
      background: white;
      border-radius: 8px;
      padding: 20px;
    }
    .form-control {
      width: 100%;
      padding: 8px;
      border: 1px solid #ddd;
      border-radius: 4px;
      box-sizing: border-box;
    }
  `]
})
export class AppComponent {
  title = 'passkey-auth-frontend';
  userIdFor2FA = '';
}

