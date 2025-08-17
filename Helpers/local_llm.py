from transformers import AutoModelForCausalLM, AutoTokenizer
import torch
import sys
import traceback
from pathlib import Path
import os

try:
    if len(sys.argv) < 2:
        print("ERROR: Prompt not provided!", file=sys.stderr)
        print("Simple answer: No question received.")

        sys.exit(1)
    
    prompt = sys.argv[1]
    
    model_path = "TinyLlama/TinyLlama-1.1B-Chat-v1.0"
    print(f"Hugging Face'den model yukleniyor: {model_path}", file=sys.stderr)
    
    tokenizer = AutoTokenizer.from_pretrained(model_path)
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token
    
    model = AutoModelForCausalLM.from_pretrained(
        model_path, 
        device_map="auto", 
        torch_dtype=torch.float16,
        low_cpu_mem_usage=True
    )
        
    inputs = tokenizer(prompt, return_tensors="pt", truncation=True, max_length=512)
    inputs = {k: v.to(model.device) for k, v in inputs.items()}
    
    with torch.no_grad():
        outputs = model.generate(
            **inputs,
            max_new_tokens=200,
            do_sample=True,
            temperature=0.7,
            top_p=0.9,
            pad_token_id=tokenizer.pad_token_id,
            eos_token_id=tokenizer.eos_token_id
        )
    
    answer = tokenizer.decode(outputs[0][inputs['input_ids'].shape[1]:], skip_special_tokens=True)
    
    if not answer.strip():
        answer = "Sorry, I cannot answer this question."
    
    print(answer.strip())

except Exception as e:
    print(f"PYTHON LLM ERROR: {str(e)}", file=sys.stderr)
    print("Sorry, a technical issue occurred and I couldn't generate a response.")

