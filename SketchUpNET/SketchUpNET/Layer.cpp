/*

SketchUpNET - a C++ Wrapper for the Trimble(R) SketchUp(R) C API
Copyright(C) 2015, Autor: Maximilian Thumfart

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

#include <SketchUpAPI/slapi.h>
#include <SketchUpAPI/geometry.h>
#include <SketchUpAPI/initialize.h>
#include <SketchUpAPI/unicodestring.h>
#include <SketchUpAPI/model/model.h>
#include <SketchUpAPI/model/entities.h>
#include <SketchUpAPI/model/face.h>
#include <SketchUpAPI/model/edge.h>
#include <SketchUpAPI/model/vertex.h>
#include <SketchUpAPI/model/layer.h>
#include <msclr/marshal.h>
#include <vector>
#include "Utilities.h"


#pragma once

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;

namespace SketchUpNET
{
	public ref class Layer
	{
	public:
		System::String^ Name;

		Layer(System::String^ name)
		{
			this->Name = name;
		};

		Layer(){};
	internal:
		static Layer^ FromSU(SULayerRef layer)
		{
			Layer^ v = gcnew Layer(SketchUpNET::Utilities::GetLayerName(layer));

			return v;
		};

		//SULayerRef ToSU()
		//{
		//	SULayerRef layer = SU_INVALID;

		//	SULayerRef* layer = (SUGroupRef*)malloc(*&size * sizeof(SUGroupRef));

		//	for (int i = 0; i < size; i++)
		//	{
		//		groups[i] = this->Groups[i]->ToSU();
		//	}
		//	SUGroupCreate(&group);
		//	return group;
		//}

		//static SUGroupRef* ListToSU(List<Group^>^ groups)
		//{
		//	size_t size = groups->Count;
		//	SUGroupRef* result = (SUGroupRef*)malloc(*&size * sizeof(SUGroupRef));
		//	for (int i = 0; i < size; i++)
		//	{
		//		result[i] = groups[i]->ToSU();
		//	}
		//	return result;
		//}

	};


}