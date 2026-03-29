import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { TwoFactorMethodService, TwoFactorMethodType } from '../../services/two-factor-method.service';

@Component({
  selector: 'app-user-create',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="user-create">
      <h2>Create User</h2>
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
      <button 
        (click)="create()" 
        [disabled]="loading || !email"
        class="btn btn-primary"
      >
        {{ loading ? 'Creating...' : 'Create User' }}
      </button>
      <div *ngIf="error" class="error">{{ error }}</div>
      <div *ngIf="success" class="success">
        {{ success }}
        <div *ngIf="createdUserId" class="user-id">
          <strong>User ID:</strong> {{ createdUserId }}
        </div>
      </div>
    </div>
  `,
  styles: [`
    .user-create {
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
    .user-id {
      margin-top: 10px;
      padding: 10px;
      background-color: #f0f0f0;
      border-radius: 4px;
      word-break: break-all;
    }
  `]
})
export class UserCreateComponent {
  email = '';
  name = '';
  loading = false;
  error?: string;
  success?: string;
  createdUserId?: string;

  constructor(private userService: UserService) {}

  create() {
    if (!this.email) {
      this.error = 'Email is required';
      return;
    }

    this.loading = true;
    this.error = undefined;
    this.success = undefined;
    this.createdUserId = undefined;

    this.userService.createUser(this.email, this.name || undefined).subscribe({
      next: (result) => {
        this.success = 'User created successfully!';
        this.createdUserId = result.id;
        this.email = '';
        this.name = '';
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to create user';
      },
      complete: () => {
        this.loading = false;
      }
    });
  }
}

