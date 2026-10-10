PDF.js（报告预览用）
====================

来源   : pdfjs-dist@3.11.174 / build 目录
文件   : pdf.min.js (320,004 B)  pdf.worker.min.js (1,087,212 B)
许可   : Apache License 2.0
用途   : 【报告】页签 · 单梁段报告的 PDF 预览与打印。

升级注意
--------
pdfjs-dist 自 v4.0 起官方只发布 ES Module（pdf.min.mjs），UMD 的 pdf.min.js 已被移除。
本项目是全局 <script> 体系（无打包器），因此锁定在 3.x 的最后一个版本。
若要升级到 v4+，必须同时改造加载方式（<script type="module"> + globalThis.pdfjsLib
+ 处理 .mjs 的 MIME 类型），不能只替换文件。

加载位置
--------
Areas/Biz/Views/ConcreteBeam/LinearControl.cshtml 的 @section Scripts，
workerSrc 由该视图通过 window.__pdfWorkerSrc 注入（配合 Url.Content，兼容虚拟目录）。
