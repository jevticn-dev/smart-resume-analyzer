import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import { ProjectDetail, CvVersionDetail } from '../models/project.models';

@Injectable({
  providedIn: 'root'
})
export class ExportService {

  exportAnalysisPdf(project: ProjectDetail, version: CvVersionDetail): void {
    const pdf = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });
    const margin = 20;
    const pageWidth = pdf.internal.pageSize.getWidth();
    const contentWidth = pageWidth - margin * 2;
    let y = margin;

    const addPage = () => {
      pdf.addPage();
      y = margin;
    };

    const checkPageBreak = (needed: number) => {
      if (y + needed > pdf.internal.pageSize.getHeight() - margin) {
        addPage();
      }
    };

    const addSectionTitle = (text: string) => {
      checkPageBreak(10);
      pdf.setFontSize(11);
      pdf.setFont('helvetica', 'bold');
      pdf.setTextColor(13, 148, 136);
      pdf.text(text, margin, y);
      y += 6;
      pdf.setDrawColor(13, 148, 136);
      pdf.setLineWidth(0.3);
      pdf.line(margin, y, margin + contentWidth, y);
      y += 5;
    };

    const addText = (text: string, fontSize = 10, bold = false) => {
      pdf.setFontSize(fontSize);
      pdf.setFont('helvetica', bold ? 'bold' : 'normal');
      pdf.setTextColor(30, 30, 30);
      const lines = pdf.splitTextToSize(text, contentWidth);
      checkPageBreak(lines.length * 5 + 2);
      pdf.text(lines, margin, y);
      y += lines.length * 5 + 2;
    };

    const addBullet = (text: string) => {
      pdf.setFontSize(10);
      pdf.setFont('helvetica', 'normal');
      pdf.setTextColor(30, 30, 30);
      const lines = pdf.splitTextToSize(text, contentWidth - 6);
      checkPageBreak(lines.length * 5 + 2);
      pdf.text('•', margin, y);
      pdf.text(lines, margin + 5, y);
      y += lines.length * 5 + 2;
    };

    // Header
    pdf.setFillColor(13, 148, 136);
    pdf.rect(0, 0, pageWidth, 28, 'F');
    pdf.setFontSize(16);
    pdf.setFont('helvetica', 'bold');
    pdf.setTextColor(255, 255, 255);
    pdf.text('Application Analysis Report', margin, 12);
    pdf.setFontSize(10);
    pdf.setFont('helvetica', 'normal');
    pdf.text(`Generated ${new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })}`, margin, 22);
    y = 38;

    // Project info
    addSectionTitle('Position Details');
    addText(`Job Title: ${project.jobTitle}`);
    addText(`Company: ${project.companyName}`);
    addText(`Industry: ${project.industry}   Seniority: ${project.seniority}`);
    y += 3;

    // CV Version info
    addSectionTitle('CV Version');
    addText(`Version ${version.versionNumber} — ${version.originalFileName}`);
    addText(`Uploaded: ${new Date(version.createdAt).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })}`);
    y += 3;

    // Match score
    if (version.analysis) {
      addSectionTitle('Match Score');
      pdf.setFontSize(36);
      pdf.setFont('helvetica', 'bold');
      pdf.setTextColor(13, 148, 136);
      checkPageBreak(20);
      y += 8;
      pdf.text(`${version.analysis.matchScore}%`, margin, y);
      y += 16;

      // Strengths
      if (version.analysis.strengths?.length) {
        addSectionTitle('Strengths');
        version.analysis.strengths.forEach(s => addBullet(s));
        y += 2;
      }

      // Weaknesses
      if (version.analysis.weaknesses?.length) {
        addSectionTitle('Weaknesses');
        version.analysis.weaknesses.forEach(w => addBullet(w));
        y += 2;
      }

      // Missing keywords
      if (version.analysis.missingKeywords?.length) {
        addSectionTitle('Missing Keywords');
        addText(version.analysis.missingKeywords.join(', '));
        y += 2;
      }

      // Suggestions
      if (version.analysis.suggestions?.length) {
        addSectionTitle('Suggestions');
        version.analysis.suggestions.forEach(s => {
          addBullet(`[${s.priority}] ${s.text}`);
        });
        y += 2;
      }

      // Summary
      if (version.analysis.summary) {
        addSectionTitle('Summary');
        addText(version.analysis.summary);
      }
    }

    pdf.save(`analysis-${project.jobTitle.replace(/\s+/g, '-').toLowerCase()}-v${version.versionNumber}.pdf`);
  }
}