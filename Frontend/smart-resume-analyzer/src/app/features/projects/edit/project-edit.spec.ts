import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectEdit } from './project-edit';

describe('Edit', () => {
  let component: ProjectEdit;
  let fixture: ComponentFixture<ProjectEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProjectEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProjectEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
